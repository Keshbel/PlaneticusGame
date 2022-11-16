using System.Collections;
using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class SpaceInvaderController : NetworkBehaviour
{
    private CurrentPlayer Player => AllSingleton.Instance.player;
    
    public SpriteRenderer spriteRenderer;
    [SyncVar(hook = nameof(UpdateColor))] public Color playerColor;

    [Header("Selecting")]
    public bool isSelecting;
    public Light2D lightShape;
    public Light2D lightLamp;
    
    [Header("Idle")]
    public bool isIdle;

    [Header("Move/Rotate")] 
    private Tweener _moveTween;
    [SyncVar] public Transform targetTransform;
    [SyncVar] public float speed = 1f;

    private void OnDisable()
    {
        if (!NetworkClient.active || Player == null) return;

        if (isClient)
        {
            Player.selectUnits.invaderControllers?.Remove(this);
            Player.CmdChangeListWithInvaders(this, false);
        }
        else Player.ChangeListWithInvaders(this, false);
        
        _moveTween?.Kill();
        StopAllCoroutines();
    }

    [Client]
    private void Update()
    {
        if (!isOwned) return;
        
        if (targetTransform != null) 
        {
            if (!isIdle) RotateTowards(targetTransform); //поворот к цели
            else IdleRotate(targetTransform.gameObject); //вращение вокруг планеты
        }
    }
    
    [Client]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Planet") || !isOwned || other.transform != targetTransform) return;
        
        if (Player.playerPlanets.Contains(other.gameObject)) // союзная планета 
        {
            var planet = other.GetComponent<PlanetController>();

            if (!planet.SpaceOrbitInvader.Contains(this))
            {
                isIdle = true;

                planet.CmdChangeOrbitInvaderList(this, true);
            }
        }
        else Attack(other.gameObject);
    }
    
    [Client]
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Planet") || !isOwned || !NetworkClient.active) return;
        if (!Player.playerPlanets.Contains(other.gameObject)) return;
        
        var planet = other.GetComponent<PlanetController>();
        if (planet.SpaceOrbitInvader.Contains(this)) planet.CmdChangeOrbitInvaderList(this, false);
    }

    #region ChangeState

    [ClientRpc]
    private void Selecting(bool isOn)
    {
        lightShape.gameObject.SetActive(isOn);
        lightLamp.gameObject.SetActive(isOn);
        isSelecting = isOn;
        if (isSelecting) StartCoroutine(LampOn());
    }
    [Command]
    public void CmdSelecting(bool isOn)
    {
        Selecting(isOn);
    }

    
    [Server]
    private void SetTargetTransform(GameObject target) //установить цель (для поворота и вращения по орбите)
    {
        targetTransform = target.transform;
    }
    [Command]
    private void CmdSetTargetTransform(GameObject target)
    {
        SetTargetTransform(target);
    }
    
    #endregion


    #region Moving & Rotation
    
    [Client]
    public void MoveTowards(GameObject target) //двигаться к (комбинация)
    {
        targetTransform = target.transform;
        var targetPos = target.transform.position;
        var distance = Vector2.Distance(targetPos, transform.position);

        if (isServer) SetTargetTransform(target);
        else CmdSetTargetTransform(target);
        
        Move(targetPos, distance / speed);
    }

    [Client]
    public void RotateTowards(Transform targetTransform) //поворот в сторону цели
    {
        var thisTransform = transform;
        Vector2 targetRotation = targetTransform.position - thisTransform.position;
        thisTransform.up = Vector2.MoveTowards(transform.up, targetRotation, Time.deltaTime * speed * 10);
    }
    
    [Client]
    private void RotateOrbit(Transform targetTransform) //вращение по орбите с поворотом
    {
        var thisTransform = transform;
        Vector2 targetRotation = targetTransform.position - thisTransform.position;
        thisTransform.up = Vector2.MoveTowards(transform.up, Vector2.Perpendicular(-targetRotation), Time.deltaTime * speed * 10);
    }

    [Client]
    private void Move(Vector3 targetPos, float duration) //движение в сторону цели
    {
        isIdle = false;
        
        _moveTween?.Kill();
        _moveTween = transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
    }
    
    [Client]
    private void IdleRotate(GameObject go) //Вращение по орбите
    {
        if (!isIdle) return;
        _moveTween?.Kill();

        transform.RotateAround(go.transform.position, Vector3.forward, 20 * speed * Time.deltaTime);
        
        RotateOrbit(targetTransform);
    }

    #endregion
    
    [Client]
    private void Attack(GameObject target)
    {
        var planet = target.GetComponent<PlanetController>();

        if (planet.SpaceOrbitInvader.Count > 0) //если есть защитники
        {
            var invaderDefender = planet.SpaceOrbitInvader[Random.Range(0, planet.SpaceOrbitInvader.Count)];
            planet.CmdChangeOrbitInvaderList(invaderDefender, false);
            
            invaderDefender.UnSpawn(invaderDefender.gameObject); //удаляем защитника
            UnSpawn(gameObject); //удаляем этого захватчика
            return;
        }

        if (planet.PlanetResources.Count > 1) //если ресурсов больше одного, то пытаемся убрать рандомный ресурс
        {
            var res = planet.PlanetResources;
            planet.CmdChangeResourceList(res[Random.Range(0, res.Count)], false);
        }
        else //если остался 1 ресурс или меньше 
        {
            //если у кого-то из игроков была планета, то удаляем из списка у него
            var playerWithPlanet = Player.players.Find(player => player.playerPlanets.Contains(target));
            if (playerWithPlanet != null) playerWithPlanet.CmdChangeListWithPlanets(target,false);

            //захват планеты
            Player.CmdChangeListWithPlanets(target, true);
        }

        //удаление захватчика
        UnSpawn(gameObject);
    }

    #region Other

    [Client]
    private void UnSpawn(GameObject go)
    {
        Player.CmdChangeListWithInvaders(this, false);
        CmdUnSpawn(go);
    }

    [Command (requiresAuthority = false)]
    private void CmdUnSpawn(GameObject go)
    {
        NetworkServer.UnSpawn(go);
        AllSingleton.Instance.invaderPoolManager.PutBackInPool(go);
    }
    
    [Client]
    private IEnumerator LampOn() //моргание лампы при выделении
    {
        if (isOwned)
            ResourceSingleton.Instance.audioLampOn.Play();
        
        lightLamp.color = Color.clear;
        yield return new WaitForSeconds(0.01f);
        lightLamp.color = Color.white;
        yield return new WaitForSeconds(0.02f);
        lightLamp.color = Color.clear;
        yield return new WaitForSeconds(0.04f);
        lightLamp.color = Color.white;
        yield return new WaitForSeconds(0.06f);
        lightLamp.color = Color.clear;
        yield return new WaitForSeconds(0.08f);
        lightLamp.color = Color.white;
        yield return new WaitForSeconds(0.10f);
        lightLamp.color = Color.clear;
        yield return new WaitForSeconds(0.12f);
        lightLamp.color = Color.white;
        yield return new WaitForSeconds(0.13f);
    }
    
    [Server]
    public void SetColor(Color newColor)
    {
        playerColor = newColor;
    }
    
    #endregion

    #region Hooks

    [Client]
    public void UpdateColor(Color oldColor, Color newColor)
    {
        spriteRenderer.color = newColor;
    }
    
    #endregion
}
