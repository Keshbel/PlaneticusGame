using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class SpaceInvaderController : NetworkBehaviour
{
    [SyncVar (hook = nameof(UpdateOwner))] public CurrentPlayer playerOwner;
    
    public SpriteRenderer spriteRenderer;
    [SyncVar(hook = nameof(UpdateColor))] public Color playerColor;
    [SyncVar(hook = nameof(UpdateSprite))] public int indexInvaderSprite;

    [Header("Selecting")]
    public bool isSelecting;
    public Light2D lightShape;
    public Light2D lightLamp;
    
    [Header("Idle")]
    [SyncVar] public bool isIdle;

    [Header("Move/Rotate")] 
    public NetworkTransform networkTransform;
    public TrailRenderer trailRenderer;
    public Tweener MoveTween;
    [SyncVar] public Transform targetTransform;
    [SyncVar] public float speed = 1f;
    
    private void OnDisable()
    {
        //if (!NetworkClient.active || playerOwner == null) return;
        targetTransform = null;
        if (isClient) CmdIsIdle(false);
        isSelecting = false;

        MoveTween?.Kill();
        StopAllCoroutines();
    }
    
    private void OnEnable()
    {
        if (isClient) CmdIsIdle(true);
        isSelecting = false;

        trailRenderer.Clear();
        //trailRenderer.time = 1.5f;
    }

    [Client]
    private void Update()
    {
        if (!isOwned && !playerOwner.isBot) return;
        
        if (targetTransform != null) 
        {
            if (!isIdle) 
            {
                //if (playerOwner.isBot) MoveTowards(targetTransform.GetComponent<PlanetController>());
                RotateTowards(targetTransform); //поворот к цели
            }
            else IdleRotate(targetTransform.gameObject); //вращение вокруг планеты
        }
    }
    
    [Client]
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Planet") || other.transform != targetTransform || playerOwner == null) return;
        if (!isOwned && !playerOwner.isBot) return;
        
        var planetController = other.gameObject.GetComponent<PlanetController>();
        
        if (playerOwner.PlayerPlanets.Contains(planetController)) // союзная планета 
        {
            if (planetController.SpaceOrbitInvader.Contains(this)) return;
            CmdIsIdle(true);
            planetController.CmdChangeOrbitInvaderList(this, true);
        }
        else Attack(planetController);
    }
    
    [Client]
    private void OnCollisionExit2D(Collision2D other)
    {
        if (!other.collider.CompareTag("Planet") || playerOwner == null) return;
        if (!playerOwner.PlayerPlanets.Contains(other.gameObject.GetComponent<PlanetController>())) return;
        
        var planet = other.gameObject.GetComponent<PlanetController>();
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
    [Command (requiresAuthority = false)]
    private void CmdSetTargetTransform(GameObject target)
    {
        SetTargetTransform(target);
    }

    [Command (requiresAuthority = false)]
    private void CmdIsIdle(bool isOn)
    {
        isIdle = isOn;
    }
    
    #endregion


    #region Moving & Rotation

    [Client]
    public void MoveTowards(PlanetController target) //двигаться к (комбинация)
    {
        //если вдруг захватчика столкнули с коллайдера планеты, то убираем его из-за захватчиков перед новой целью.
        if (targetTransform != null)
            targetTransform.GetComponent<PlanetController>().CmdChangeOrbitInvaderList(this, false);

        targetTransform = target.transform;
        var targetPos = targetTransform.position;
        var distance = Vector2.Distance(targetPos, transform.position);

        CmdSetTargetTransform(target.gameObject);
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
    private async void Move(Vector3 targetPos, float duration) //движение в сторону цели
    {
        CmdIsIdle(false);
        
        MoveTween?.Kill();
        MoveTween = transform.DOMove(targetPos, duration).SetEase(Ease.Linear);
        await MoveTween.AsyncWaitForKill();
        MoveTween = null;
    }
    
    [Client]
    private void IdleRotate(GameObject go) //Вращение по орбите
    {
        if (!isIdle) return;
        MoveTween?.Kill();

        transform.RotateAround(go.transform.position, Vector3.forward, 20 * speed * Time.deltaTime);
        RotateOrbit(targetTransform);
    }

    #endregion
    
    [Client]
    private async void Attack(PlanetController targetPlanet)
    {
        if (!isOwned && !playerOwner.isBot) return;
        
        if (targetPlanet.SpaceOrbitInvader.Count > 0) //если есть защитники
        {
            var invaderDefender = targetPlanet.SpaceOrbitInvader[Random.Range(0, targetPlanet.SpaceOrbitInvader.Count)];
            targetPlanet.CmdChangeOrbitInvaderList(invaderDefender, false);
            
            invaderDefender.CmdUnSpawn(invaderDefender.gameObject); //удаляем защитника
            CmdUnSpawn(gameObject); //удаляем этого захватчика
            return;
        }

        if (!targetPlanet.isHomePlanet)
        {
            var res = targetPlanet.PlanetResources;
            
            if (res.Count > 1) //если ресурсов больше одного, то пытаемся убрать рандомный ресурс
            {
                targetPlanet.CmdChangeResourceList(targetPlanet.PlanetResources[0], false);
            }
            else //если остался 1 ресурс или меньше 
            {
                //если у кого-то из игроков была планета, то удаляем из списка у него
                var playerWithPlanet = playerOwner.players.Find(player => player.PlayerPlanets.Contains(targetPlanet));
                if (playerWithPlanet != null) playerWithPlanet.CmdChangeListWithPlanets(targetPlanet, false);

                //захват планеты
                playerOwner.CmdChangeListWithPlanets(targetPlanet, true);
            }
        }
        else
        {
            targetPlanet.CmdChangeCountToDestroy(1, false);
            
            if (targetPlanet.countToDestroy < 1)
            {
                var playerWithPlanet = playerOwner.players.Find(p => p.PlayerPlanets.Contains(targetPlanet));
                if (playerWithPlanet != null)
                {
                    if (playerWithPlanet.isBot) playerWithPlanet.Defeat(playerOwner);
                    else playerWithPlanet.CmdDefeat(playerOwner); //поражение другого игрока/бота
                }

                await Task.Delay(350);
                var playersLive = playerOwner.players.Where(player => player.PlayerPlanets.Count > 0);
                if (playersLive.Count() == 1) AllSingleton.Instance.endGame.VictoryResult(); //победа, если остался один
            }
        }
        
        //удаление захватчика
        //transform.position = Vector3.zero;
        CmdUnSpawn(gameObject);
    }
    #region Other

    [Command (requiresAuthority = false)]
    public void CmdUnSpawn(GameObject go)
    {
        //transform.position = Vector3.zero;
        playerOwner.ChangeListWithInvaders(this, false);
        targetTransform = null;
        playerColor = Color.clear;
        playerOwner = null;
        NetworkServer.UnSpawn(go);
        AllSingleton.Instance.invaderPoolManager.PutBackInPool(go);
    }

    [Client]
    private IEnumerator LampOn() //моргание лампы при выделении
    {
        if (isOwned || playerOwner.isBot) ResourceSingleton.Instance.audioLampOn.Play();
        
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
    
    [Client]
    public void UpdateSprite(int oldInt, int newInt)
    {
        spriteRenderer.sprite = ResourceSingleton.Instance.invaderSprites[newInt];
    }

    [Client]
    public void UpdateOwner(CurrentPlayer oldPlayer, CurrentPlayer newPlayer)
    {
        if (newPlayer != null) networkTransform.syncDirection = newPlayer.isBot ? SyncDirection.ServerToClient : SyncDirection.ClientToServer;
        else if (oldPlayer != null) networkTransform.syncDirection = oldPlayer.isBot ? SyncDirection.ServerToClient : SyncDirection.ClientToServer;
    }
    
    #endregion
}
