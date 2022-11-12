using System.Collections;
using DG.Tweening;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class SpaceInvaderController : NetworkBehaviour
{
    [SyncVar]
    public NetworkIdentity playerIdentity;

    [Header("Selecting")]
    public bool isSelecting;
    public Light2D lightShape;
    public Light2D lightLamp;
    
    [Header("Idle")]
    [SyncVar]
    public bool isIdle;
    [SyncVar]
    public bool isRotateOrbit;

    [Header("Move/Rotate")] 
    public Tweener MoveTween;
    [SyncVar]
    public Transform targetTransform;
    [SyncVar]
    public float speed = 1f;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (hasAuthority)
        {
            var identity = NetworkClient.connection.identity;
            
            if (isServer)
            {
                SetPlayer(identity);
            }
            else
            {
                CmdSetPlayer(identity);
            }
        }
    }

    private void OnDestroy()
    {
        MoveTween?.Kill();
        MoveTween = null;
        StopAllCoroutines();

        if (AllSingleton.Instance.player)
        {
            AllSingleton.Instance.player.selectUnits.invaderControllers?.Remove(this);
            if (isServer) AllSingleton.Instance.player.ChangeListWithInvaders(this, false);
            else AllSingleton.Instance.player.CmdChangeListWithInvaders(this, false);
        }
    }

    private void Update()
    {
        if (hasAuthority)
        {
            if (targetTransform != null && !isIdle) //поворот к цели
            {
                if (isServer)
                    RotateTowards();
                else
                {
                    CmdRotateTowards();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isServer)
                    SetIdle(true);
                else
                {
                    CmdSetIdle(true);
                }
            }

            if (isIdle && targetTransform != null) //вращение вокруг планеты
            {
                if (isServer)
                    IdleRotate(targetTransform.gameObject);
                else
                {
                    CmdIdleRotate(targetTransform.gameObject);
                }
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Planet") && hasAuthority)
        {
            if (AllSingleton.Instance.player.playerPlanets.Contains(other.gameObject)) // союзная планета 
            {
                if (other.transform == targetTransform)
                {
                    var planet = other.GetComponent<PlanetController>();
                    
                    if (isServer)
                    {
                        SetIdle(true);
                        SetRotateOrbit(true);
                        if (!planet.SpaceOrbitInvader.Contains(this))
                            planet.ChangeOrbitInvaderList(this, true);
                    }
                    else
                    {
                        CmdSetIdle(true);
                        CmdSetRotateOrbit(true);
                        if (!planet.SpaceOrbitInvader.Contains(this))
                            planet.CmdChangeOrbitInvaderList(this, true);
                    }
                }
            }
            
            else
            {
                if (other.transform == targetTransform)
                {
                    if (isServer)
                        Attack(other.gameObject);
                    else
                    {
                        CmdAttack(other.gameObject);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Planet") && hasAuthority)
        {
            if (AllSingleton.Instance.player.playerPlanets.Contains(other.gameObject)) // союзная планета 
            {
                var planet = other.GetComponent<PlanetController>();
                
                if (isServer)
                {
                    if (planet.SpaceOrbitInvader.Contains(this))
                        planet.ChangeOrbitInvaderList(this, false);
                }
                else
                {
                    if (planet.SpaceOrbitInvader.Contains(this))
                        planet.CmdChangeOrbitInvaderList(this, false);
                }
            }
        }
    }
    

    [Server]
    private void SetPlayer(NetworkIdentity identity)
    {
        if (playerIdentity == null)
            playerIdentity = identity;
    }
    [Command]
    private void CmdSetPlayer(NetworkIdentity identity)
    {
        SetPlayer(identity);
    }

    private IEnumerator LampOn() //моргание лампы при выделении
    {
        if (hasAuthority)
            ResourceSingleton.instance.audioLampOn.Play();
        
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
    
    #region ChangeState
    
    [Server]
    public void SetIdle(bool isOn)
    {
        isIdle = isOn;
    }
    [Command]
    public void CmdSetIdle(bool isOn)
    {
        SetIdle(isOn);
    }

    
    [Server]
    public void SetRotateOrbit(bool isOn)
    {
        isRotateOrbit = isOn;
    }
    [Command]
    public void CmdSetRotateOrbit(bool isOn)
    {
        SetRotateOrbit(isOn);
    }
    

    [ClientRpc]
    public void Selecting(bool isOn)
    {
        lightShape.gameObject.SetActive(isOn);
        lightLamp.gameObject.SetActive(isOn);
        isSelecting = isOn;
        if (isSelecting)
            StartCoroutine(LampOn());
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
    
    public void MoveTowards(GameObject target) //двигаться к (комбинация)
    {
        targetTransform = target.transform;
        var targetPos = target.transform.position;
        var distance = Vector2.Distance(targetPos, transform.position);

        if (isServer)
        {
            SetTargetTransform(target);
            Move(targetPos, distance / speed);
        }
        else
        {
            CmdSetTargetTransform(target);
            CmdMove(targetPos, distance / speed);
        }
    }

    [Server]
    public void StopMoving()
    {
        MoveTween?.Kill();
    }
    [Command]
    public void CmdStopMoving()
    {
        StopMoving();
    }

    [Server]
    public void RotateTowards() //поворот в сторону цели
    {
        var thisTransform = transform;
        Vector2 targetRotation = targetTransform.position - thisTransform.position;
        thisTransform.up = Vector2.MoveTowards(transform.up, targetRotation, Time.deltaTime * speed * 10);
    }
    [Command]
    public void CmdRotateTowards()
    {
        RotateTowards();
    }

    
    [Server]
    public void Move(Vector3 targetPos, float duration) //движение в сторону цели
    {
        SetIdle(false);
        MoveTween?.Kill();
        MoveTween = transform.DOMove(targetPos, duration);
    }
    [Command]
    public void CmdMove(Vector3 targetPos, float duration)
    {
        Move(targetPos, duration);
    }
    
    
    [Server]
    public void IdleRotate(GameObject GO) //Вращение по орбите
    {
        if (!isIdle) return;
        MoveTween?.Kill();

        transform.RotateAround(GO.transform.position, Vector3.forward, 20 * speed * Time.deltaTime);
        
        if (isRotateOrbit) //первый поворот (для нужного угла вращения)
        {
            var rotation = transform.localRotation;
            transform.DOLocalRotate(new Vector3(rotation.x, rotation.y, rotation.z - 70), 1f, RotateMode.LocalAxisAdd);
            isRotateOrbit = false;
        }
    }
    [Command]
    public void CmdIdleRotate(GameObject GO)
    {
        IdleRotate(GO);
    }
    
    #endregion

    
    [Server]
    public void Attack(GameObject target)
    {
        var player = playerIdentity.GetComponent<CurrentPlayer>();
        var planet = target.GetComponent<PlanetController>();

        if (planet.SpaceOrbitInvader.Count > 0) //если есть защитники
        {
            var invaderDefender = planet.SpaceOrbitInvader[Random.Range(0, planet.SpaceOrbitInvader.Count)];
            planet.ChangeOrbitInvaderList(invaderDefender, false);
            //удаляем защитника
            NetworkServer.Destroy(invaderDefender.gameObject);

            //удаляем этого захватчика
            NetworkServer.Destroy(gameObject);
            return;
        }
        
        if (planet.PlanetResources.Count > 1) //если ресурсов больше одного, то пытаемся убрать рандомный ресурс
        {
            planet.ChangeResourceList(planet.PlanetResources[Random.Range(0, planet.PlanetResources.Count)], false);
        }
        else //если остался 1 или меньше
        {
            //если у кого-то из игроков была планета, то удаляем из списка у него
            foreach (var thisPlayerIdentity in AllSingleton.Instance.currentPlayers)
            {
                var thisPlayer = thisPlayerIdentity.GetComponent<CurrentPlayer>();

                if (thisPlayer.playerPlanets.Find(p => p == target))
                {
                    thisPlayer.ChangeListWithPlanets(target, false);
                    break;
                }
            }
            
            //захват планеты
            player.ChangeListWithPlanets(target, true);
            if (isServer)
            {
                planet.Colonization();
            }
        }

        //удаление захватчика
        NetworkServer.Destroy(gameObject);
    }
    [Command]
    public void CmdAttack(GameObject target)
    {
        Attack(target);
    }
}
