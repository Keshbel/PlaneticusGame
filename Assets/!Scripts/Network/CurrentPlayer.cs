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
    public SyncList<SpaceInvaderController> playerInvaders = new SyncList<SpaceInvaderController>();
    
    [SyncVar] public SelectUnits selectUnits;

    private void Start()
    {
        if (hasAuthority)
        {
            if (!selectUnits && isClient)
            {
                CmdCreateSelectUnits();
            }
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        AddUserIdInList();

        if (hasAuthority)
        {
            AllSingleton.Instance.player = this;
            if (isServer)
            {
                SetPlayerColor();
                Invoke(nameof(HomePlanetAddingToPlayer), 0.5f);
            }
            else
            {
                CmdSetPlayerColor();
                Invoke(nameof(CmdHomePlanetAddingToPlayer), 0.5f);
                Invoke(nameof(CameraToHome), 1f);
            }
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        foreach (var planet in playerPlanets)
        {
            if (isServer)
                AllSingleton.Instance.mainPlanetController.RemovePlanetFromListPlanet(planet.GetComponent<PlanetController>());
            else
            {
                AllSingleton.Instance.mainPlanetController.CmdRemovePlanetFromListPlanet(planet.GetComponent<PlanetController>());
            }
        }

        if (isServer)
            AllSingleton.Instance.RemovePlayer(gameObject);
        else
        {
            AllSingleton.Instance.CmdRemovePlayer(gameObject);
        }
    }

    

    #region User
    public void AddUserIdInList()
    {
        if (isClient)
        {
            AllSingleton.Instance.CmdAddPlayer(gameObject);
        }
        if (isServer)
        {
            AllSingleton.Instance.AddPlayer(gameObject);
        }
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
            planet.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
            planet.GetComponent<PlanetController>().colorPlanet = playerColor;
        }
        else
        {
            playerPlanets.Remove(planet);
            planet.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            planet.GetComponent<PlanetController>().colorPlanet = Color.white;
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
    public void ChangeListWithInvaders(SpaceInvaderController invader, bool isAdding)
    {
        if (isAdding)
        {
            playerInvaders?.Add(invader);
        }
        else
        {
            playerInvaders?.Remove(invader);
        }
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
            invaderControllerComponent.targetTransform = goPosition.transform;
            
            invader.transform.position = new Vector3(xBoundCollider, planetSpawnPosition.y, planetSpawnPosition.z);
            
            if (isServer)
            {
                ChangeListWithInvaders(invaderControllerComponent, true);
            }
            else
            {
                CmdChangeListWithInvaders(invaderControllerComponent, true);
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
            AllSingleton.Instance.cameraMove.DoMove(position.x, position.y, 1f);
    }
    #endregion
}
