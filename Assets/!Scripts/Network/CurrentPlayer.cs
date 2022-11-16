using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

[Serializable]
public class CurrentPlayer : NetworkBehaviour
{
    [SyncVar]
    public string playerName;

    [SyncVar] 
    public Color playerColor;
    
    [Header("SyncLists")]
    public readonly SyncList<GameObject> playerPlanets = new SyncList<GameObject>();
    public List<CurrentPlayer> players = new List<CurrentPlayer>();
    public readonly SyncList<SpaceInvaderController> PlayerInvaders = new SyncList<SpaceInvaderController>();
    
    [SyncVar] public SelectUnits selectUnits;

    private void Start()
    {
        if (isOwned)
        {
            if (!selectUnits && isClient)
            {
                CmdCreateSelectUnits();
            }
        }
        
        Invoke(nameof(AddPlayer), 1f);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isOwned)
        {
            AllSingleton.Instance.player = this;
            
            var roomPlayers = FindObjectsOfType<RoomPlayer>().ToList();
            var roomPlayer = roomPlayers.Find(p => p.isOwned);

            if (isServer)
            {
                SetPlayerColor(roomPlayer);
                Invoke(nameof(HomePlanetAddingToPlayer), 0.5f);
            }
            else
            {
                CmdSetPlayerColor(roomPlayer);
                Invoke(nameof(CmdHomePlanetAddingToPlayer), 0.5f);
                Invoke(nameof(CameraToHome), 1f);
                
            }
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (!NetworkServer.active || AllSingleton.Instance == null) return;
        
        foreach (var planet in playerPlanets)
        {
            if (isServer) AllSingleton.Instance.mainPlanetController.RemovePlanetFromListPlanet(planet.GetComponent<PlanetController>());
            else AllSingleton.Instance.mainPlanetController.CmdRemovePlanetFromListPlanet(planet.GetComponent<PlanetController>());
        }

    }
    
    #region User
    
    public void AddPlayer()
    {
        players = FindObjectsOfType<CurrentPlayer>().ToList();
    }
    
    [Command]
    public void CmdCreateSelectUnits()
    {
        var sU = Instantiate(AllSingleton.Instance.selectUnitsPrefab);
        NetworkServer.Spawn(sU, connectionToClient);
        selectUnits = sU.GetComponent<SelectUnits>();
    }
    
    #endregion

    #region Planets
    [Server]
    public void ChangeListWithPlanets(GameObject planet, bool isAdding)
    {
        if (isAdding)
        {
            playerPlanets.Add(planet);
            
            var identity = planet.GetComponent<NetworkIdentity>();
            identity.RemoveClientAuthority();
            identity.AssignClientAuthority(connectionToClient);
            
            var planetController = planet.GetComponent<PlanetController>();
            planetController.colorPlanet = playerColor;
            planetController.Colonization();
        }
        else
        {
            playerPlanets.Remove(planet);
            
            planet.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            planet.GetComponent<PlanetController>().colorPlanet = Color.white;
        }
    }
    [Command (requiresAuthority = false)]
    public void CmdChangeListWithPlanets(GameObject planet, bool isAdding)
    {
        ChangeListWithPlanets(planet, isAdding);
    }
    #endregion

    #region Invaders
    [Server]
    public void ChangeListWithInvaders(SpaceInvaderController invader, bool isAdding)
    {
        if (isAdding) PlayerInvaders?.Add(invader);
        else PlayerInvaders?.Remove(invader);
        
        PlayerInvaders?.RemoveAll(invaderNull => invaderNull == null);
    }
    [Command]
    public void CmdChangeListWithInvaders(SpaceInvaderController invader, bool isAdding)
    {
        ChangeListWithInvaders(invader, isAdding);
    }

    [Server]
    public IEnumerator SpawnInvader(int count, GameObject goPosition) //спавн захватчиков (может стоит перенести в отдельный скрипт?)
    {
        for (int i = 0; i < count; i++)
        {
            var invader = Instantiate(AllSingleton.Instance.invaderPrefab);

            var planetSpawnPosition = goPosition.transform.position; //позиция планеты
            var xBoundCollider = goPosition.GetComponent<CircleCollider2D>().bounds.max.x; //граница коллайдера по х

            var invaderControllerComponent = invader.GetComponent<SpaceInvaderController>();
            invaderControllerComponent.SetColor(playerColor);
            invaderControllerComponent.targetTransform = goPosition.transform;
            
            invader.transform.position = new Vector3(xBoundCollider, planetSpawnPosition.y, planetSpawnPosition.z);
            
            ChangeListWithInvaders(invaderControllerComponent, true);

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
        var listPlanet = AllSingleton.Instance.mainPlanetController.listPlanet;
        //домашняя планета
        if (listPlanet.Count > 0)
        {
            var homePlanet = listPlanet.Find(planet=>!planet.isHomePlanet);
            homePlanet.SetHomePlanet();

            if (isServer)
            {
                ChangeListWithPlanets(homePlanet.gameObject, true);
                homePlanet.Colonization();
                StartCoroutine(SpawnInvader(2, homePlanet.gameObject));
            }
            else
            {
                CmdChangeListWithPlanets(homePlanet.gameObject, true);
                homePlanet.CmdColonization();
                CmdSpawnInvader(2, homePlanet.gameObject);
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
    public void SetPlayerColor(RoomPlayer roomPlayer)
    {
        playerColor = roomPlayer.playerColor;
    }
    [Command]
    public void CmdSetPlayerColor(RoomPlayer roomPlayer)
    {
        SetPlayerColor(roomPlayer);
    }

    [Client]
    public void CameraToHome()
    {
        var position = playerPlanets[0].transform.position;
        if (isOwned && NetworkClient.connection.identity.GetComponent<CurrentPlayer>() == this)
            AllSingleton.Instance.cameraMove.DoMove(position.x, position.y, 1f);
    }
    #endregion
}
