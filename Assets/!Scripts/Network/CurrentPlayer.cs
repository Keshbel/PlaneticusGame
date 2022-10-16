using System;
using System.Collections;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class CurrentPlayer : NetworkBehaviour
{
    [SyncVar]
    public string playerName;

    [SyncVar] 
    public Color playerColor;
    
    [Header("SyncLists")]
    public SyncList<GameObject> playerPlanets = new SyncList<GameObject>();
    public SyncList<GameObject> playerInvaders = new SyncList<GameObject>();

    public SpaceInvaderController invaderController;
    public PlanetController targetPlanet;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        AddUserIdInList();

        if (hasAuthority)
        {
            AllSingleton.instance.player = this;
            if (isServer)
            {
                SetPlayerColor();
                Invoke(nameof(HomePlanetAddingToPlayer), 0.5f);
            }
            else
            {
                CmdSetPlayerColor();
                Invoke(nameof(CmdHomePlanetAddingToPlayer), 0.5f);
                Invoke(nameof(CameraToHome), 0.6f);
            }
        }
    }

    private void Update()
    {
        if (hasAuthority)
        {
            if (Input.GetMouseButtonDown(0)) //при клике левой кнопкой
            {
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
                
                //при попадании по объекту с колайдером
                if (hit.collider != null) 
                {
                    var invader = hit.collider.GetComponent<SpaceInvaderController>();
                    targetPlanet = hit.collider.GetComponent<PlanetController>();

                    if (invader != null && playerInvaders.Contains(invader.gameObject)) //если клик был по захватчику, то выделяем его
                    {
                        //снимаем выделение с прошлого захватчика, если был
                        if (invaderController != null)
                        {
                            if (isServer)
                                invaderController.Selecting(false);
                            else
                            {
                                invaderController.CmdSelecting(false);
                            }
                        }
                        
                        //назначаем и выделяем нового захватчика
                        invaderController = invader;
                        if (isServer)
                            invaderController.Selecting(true);
                        else
                        {
                            invaderController.CmdSelecting(true);
                        }
                    }
                    
                    // если есть цель, выбран захватчик и дистанция не слишком маленькая, то движемся к цели
                    if (targetPlanet != null && invaderController != null && Vector2.Distance
                        (invaderController.transform.position, targetPlanet.transform.position) > 1.7)
                    {
                        invaderController.MoveTowards(targetPlanet.gameObject);
                    }
                }
                
                //при попадании по объекту без коллайдера, снимаем выделение
                else
                {
                    if (invaderController != null)
                    {
                        if (isServer)
                            invaderController.Selecting(false);
                        else
                        {
                            invaderController.CmdSelecting(false);
                        }

                        invaderController = null;
                    }
                }
            }
        }
    }

    #region User
    public void AddUserIdInList()
    {
        if (isClient)
        {
            AllSingleton.instance.CmdAddPlayer(GetComponent<NetworkIdentity>());
        }
        if (isServer)
        {
            AllSingleton.instance.AddPlayer(GetComponent<NetworkIdentity>());
        }
    }
    #endregion

    #region Planets
    [Server]
    public void ChangeListWithPlanets(GameObject planet, bool isAdding)
    {
        if (isAdding)
        {
            playerPlanets.Add(planet);
            planet.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
            planet.GetComponent<PlanetController>().textName.color = playerColor;
        }
        else
        {
            playerPlanets.Remove(planet);
            planet.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            planet.GetComponent<PlanetController>().textName.color = Color.white;
        }
    }
    [Command]
    public void CmdChangeListWithPlanets(GameObject planet, bool isAdding)
    {
        ChangeListWithPlanets(planet, isAdding);
    }
    #endregion

    #region Invaders
    [Server]
    public void ChangeListWithInvaders(GameObject invader, bool isAdding)
    {
        if (isAdding)
        {
            playerInvaders.Add(invader);
        }
        else
        {
            playerInvaders.Remove(invader);
        }
    }
    [Command]
    public void CmdChangeListWithInvaders(GameObject invader, bool isAdding)
    {
        ChangeListWithPlanets(invader, isAdding);
    }

    [Server]
    public IEnumerator SpawnInvader(int count, GameObject goPosition) //спавн захватчиков (может стоит перенести в отдельный скрипт?)
    {
        for (int i = 0; i < count; i++)
        {
            var invader = Instantiate(AllSingleton.instance.invaderPrefab);

            var planetSpawnPosition = goPosition.transform.position; //позиция планеты
            var xBoundCollider = goPosition.GetComponent<CircleCollider2D>().bounds.max.x; //граница коллайдера по х

            var invaderControllerComponent = invader.GetComponent<SpaceInvaderController>();
            invaderControllerComponent.targetTransform = goPosition.transform;
            
            invader.transform.position = new Vector3(xBoundCollider, planetSpawnPosition.y, planetSpawnPosition.z);
            
            if (isServer)
            {
                ChangeListWithInvaders(invader, true);
            }
            else
            {
                CmdChangeListWithInvaders(invader, true);
            }
            NetworkServer.Spawn(invader, connectionToClient);
            
            yield return new WaitForSeconds(2f); 
        }
    }
    [Command]
    public void CmdSpawnInvader(int count, GameObject goPosition)
    {
        StartCoroutine(SpawnInvader(count, goPosition));
    }
    #endregion

    public void HomePlanetAddingToPlayer()
    {
        var listPlanet = AllSingleton.instance.mainPlanetController.listPlanet;
        //домашняя планета
        if (listPlanet.Count > 0)
        {
            var homePlanet = listPlanet.Find(planet=>!planet.isHomePlanet);
            homePlanet.SetHomePlanet();

            if (isServer)
            {
                ChangeListWithPlanets(homePlanet.gameObject, true);
                homePlanet.HomingPlanetShow();
                homePlanet.RpcResourceIconShow();
                homePlanet.Colonization();
                StartCoroutine(SpawnInvader(3, homePlanet.gameObject));
            }
            else
            {
                CmdChangeListWithPlanets(homePlanet.gameObject, true);
                homePlanet.CmdHomingPlanetShow();
                homePlanet.RpcResourceIconShow();
                homePlanet.CmdColonization();
                CmdSpawnInvader(3, homePlanet.gameObject);
            }
            
            CameraToHome();
        }
    }
    [Command]
    public void CmdHomePlanetAddingToPlayer()
    {
        HomePlanetAddingToPlayer();
    }

    #region Other

    [Server]
    public void SetPlayerColor()
    {
        playerColor = Random.ColorHSV();
    }
    [Command]
    public void CmdSetPlayerColor()
    {
        SetPlayerColor();
    }
    
    [Client]
    public void CameraToHome()
    {
        var position = playerPlanets[0].transform.position;
        if (hasAuthority && NetworkClient.connection.identity.GetComponent<CurrentPlayer>() == this)
            AllSingleton.instance.cameraMove.DoMove(position.x, position.y, 1f);
    }
    #endregion
}
