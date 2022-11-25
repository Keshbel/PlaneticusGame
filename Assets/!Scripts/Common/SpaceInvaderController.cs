using System.Collections;
using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class SpaceInvaderController : NetworkBehaviour
{
    [SyncVar] public CurrentPlayer player;
    
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

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!player && isOwned) CmdSetPlayer(NetworkClient.connection.identity.GetComponent<CurrentPlayer>());
    }

    private void OnDisable()
    {
        if (!NetworkClient.active || player == null) return;

        if (isClient)
        {
            player.selectUnits.invaderControllers?.Remove(this);
            player.CmdChangeListWithInvaders(this, false);
        }
        else player.ChangeListWithInvaders(this, false);
        
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
        
        var planetController = other.GetComponent<PlanetController>();
        
        if (player.PlayerPlanets.Contains(planetController)) // союзная планета 
        {
            if (!planetController.SpaceOrbitInvader.Contains(this))
            {
                isIdle = true;

                planetController.CmdChangeOrbitInvaderList(this, true);
            }
        }
        else Attack(planetController);
    }
    
    [Client]
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Planet") || !isOwned || !NetworkClient.active) return;
        if (!player.PlayerPlanets.Contains(other.GetComponent<PlanetController>())) return;
        
        var planet = other.GetComponent<PlanetController>();
        if (planet.SpaceOrbitInvader.Contains(this) && targetTransform != other.transform) 
            planet.CmdChangeOrbitInvaderList(this, false);
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
        //если вдруг захватчика столкнули с коллайдера планеты, то убираем его из-за захватчиков перед новой целью.
        targetTransform.GetComponent<PlanetController>().CmdChangeOrbitInvaderList(this,  false); 
        
        targetTransform = target.transform;
        var targetPos = target.transform.position;
        var distance = Vector2.Distance(targetPos, transform.position);
        
        CmdSetTargetTransform(target);
        Move(targetPos, distance / speed);
    }

    [Client]
    public void RotateTowards(Transform target) //поворот в сторону цели
    {
        var thisTransform = transform;
        Vector2 targetRotation = target.position - thisTransform.position;
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
    private void Attack(PlanetController target)
    {
        var planet = target.GetComponent<PlanetController>();

        if (planet.SpaceOrbitInvader.Count > 0) //если есть защитники
        {
            var invaderDefender = planet.SpaceOrbitInvader[Random.Range(0, planet.SpaceOrbitInvader.Count)];
            planet.CmdChangeOrbitInvaderList(invaderDefender, false);
            
            invaderDefender.CmdUnSpawn(invaderDefender.gameObject); //удаляем защитника
            CmdUnSpawn(gameObject); //удаляем этого захватчика
            return;
        }

        if (!planet.isHomePlanet)
        {
            if (planet.PlanetResources.Count > 1) //если ресурсов больше одного, то пытаемся убрать рандомный ресурс
            {
                var res = planet.PlanetResources;
                planet.CmdChangeResourceList(res[Random.Range(0, res.Count)], false);
            }
            else //если остался 1 ресурс или меньше 
            {
                //если у кого-то из игроков была планета, то удаляем из списка у него
                var playerWithPlanet = player.players.Find(player => player.PlayerPlanets.Contains(target));
                if (playerWithPlanet != null) playerWithPlanet.CmdChangeListWithPlanets(target, false);

                //захват планеты
                player.CmdChangeListWithPlanets(target, true);
            }
        }
        else
        {
            if (planet.countToDestroy > 1) planet.CmdReduceCountToDestroy();
            else
            {
                var playerWithPlanet = player.players.Find(p => p.PlayerPlanets.Contains(target));
                if (playerWithPlanet != null) playerWithPlanet.Lose(player);
            }
        }
        
        //удаление захватчика
        CmdUnSpawn(gameObject);
    }

    #region Other

    [Command]
    private void CmdSetPlayer(CurrentPlayer newPlayer)
    {
        player = newPlayer;
    }

    [Command (requiresAuthority = false)]
    public void CmdUnSpawn(GameObject go)
    {
        player.ChangeListWithInvaders(this, false);
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
