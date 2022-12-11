using System;
using System.Collections.Generic;
using System.Linq;
using Lean.Localization;
using Mirror;
using UnityEngine;

[Serializable]
public class CurrentPlayer : NetworkBehaviour
{
    public List<CurrentPlayer> players = new List<CurrentPlayer>();
    
    [Header("Main")]
    [SyncVar] public string playerName;
    [SyncVar] public Color playerColor;
    [SyncVar] public int indexInvaderSprite;

    [SyncVar] public CurrentPlayer enemyPlayerDefeat;

    [Header("SyncLists")]
    public readonly SyncList<PlanetController> PlayerPlanets = new SyncList<PlanetController>();
    public readonly SyncList<SpaceInvaderController> PlayerInvaders = new SyncList<SpaceInvaderController>();
    
    [SyncVar] public SelectUnits selectUnits;

    [Client]
    private void Start()
    {
        if (isOwned && !selectUnits && isClient)
        {
            CmdCreateSelectUnits();
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isOwned) return;

        AllSingleton.Instance.player = this;

        var roomPlayers = FindObjectsOfType<RoomPlayer>().ToList();
        var roomPlayer = roomPlayers.Find(p => p.isOwned);

        CmdSetPlayerData(roomPlayer);
        Invoke(nameof(CmdHomePlanetAddingToPlayer), 0.5f);
        Invoke(nameof(CameraToHome), 1f);
        Invoke(nameof(AddPlayer), 1f);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        
        players.Clear();
        AddPlayer();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (!NetworkServer.active || AllSingleton.Instance == null) return;
        
        foreach (var planet in PlayerPlanets)
        {
            if (isServer) AllSingleton.Instance.mainPlanetController.RemovePlanetFromList(planet.GetComponent<PlanetController>());
            else AllSingleton.Instance.mainPlanetController.CmdRemovePlanetFromList(planet.GetComponent<PlanetController>());
        }
    }
    
    #region User
    
    [Client]
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
    public void ChangeListWithPlanets(PlanetController planet, bool isAdding)
    {
        if (isAdding)
        {
            PlayerPlanets.Add(planet);
            
            var identity = planet.GetComponent<NetworkIdentity>();
            identity.RemoveClientAuthority();
            identity.AssignClientAuthority(connectionToClient);
            
            var planetController = planet.GetComponent<PlanetController>();
            planetController.colorPlanet = playerColor;
            planetController.Colonization();
        }
        else
        {
            PlayerPlanets.Remove(planet);
            
            planet.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            planet.GetComponent<PlanetController>().colorPlanet = Color.white;
        }
    }
    [Command (requiresAuthority = false)]
    public void CmdChangeListWithPlanets(PlanetController planet, bool isAdding)
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
        
        
        //var identity = invader.GetComponent<NetworkIdentity>();
        /*identity.RemoveClientAuthority();
        identity.AssignClientAuthority(connectionToClient);*/
    }
    [Command (requiresAuthority = false)]
    public void CmdChangeListWithInvaders(SpaceInvaderController invader, bool isAdding)
    {
        ChangeListWithInvaders(invader, isAdding);
    }

    [Server]
    public void SpawnInvader(int count, GameObject goPosition) //спавн захватчиков (может стоит перенести в отдельный скрипт?)
    {
        for (int i = 0; i < count; i++)
        {
            var xBoundCollider = goPosition.GetComponent<CircleCollider2D>().bounds.max.x; //граница коллайдера по х
            var planetPosition = goPosition.transform.position; //позиция планеты
            var spawnPosition = new Vector3(xBoundCollider, planetPosition.y, planetPosition.z);

            var invader = AllSingleton.Instance.invaderPoolManager.GetFromPool(spawnPosition, Quaternion.identity);
            NetworkServer.Spawn(invader, connectionToClient);

            var invaderControllerComponent = invader.GetComponent<SpaceInvaderController>();
            invaderControllerComponent.playerOwner = connectionToClient.identity.GetComponent<CurrentPlayer>();
            invaderControllerComponent.indexInvaderSprite = indexInvaderSprite;
            invaderControllerComponent.SetColor(playerColor);
            invaderControllerComponent.targetTransform = goPosition.transform;
            
            ChangeListWithInvaders(invaderControllerComponent, true);
        }
    }

    [Command]
    public void CmdSpawnInvader(int count, GameObject goPosition)
    {
        SpawnInvader(count, goPosition);
    }
    #endregion

    [Command (requiresAuthority = false)]
    public void CmdDefeat(CurrentPlayer playerInvader)
    {
        Defeat(playerInvader);
    }
    [TargetRpc]
    public void Defeat(CurrentPlayer playerInvader)
    {
        print("Я пытаюсь выполниться (поражение)");
        PlayerPlanets.ToList().ForEach(planet => //добавляем имущество победителю
        {
            CmdChangeListWithPlanets(planet,false);
            playerInvader.CmdChangeListWithPlanets(planet,true);
        });
        PlayerInvaders.ToList().ForEach(invader => invader.CmdUnSpawn(invader.gameObject));

        //оповощение о поражении
        CmdSendLoseMessage(playerInvader);
        //включение UI поражения
        AllSingleton.Instance.endGame.DefeatResult();
    }

    [Command (requiresAuthority = false)]
    public void CmdSetPlayerNameInvader(CurrentPlayer playerInvader)
    {
        enemyPlayerDefeat = playerInvader;
    }
    [Server]
    public void SendLoseMessage(CurrentPlayer playerInvader)
    {
        RpcSendLoseMessage(playerInvader);
    }
    [Command (requiresAuthority = false)]
    public void CmdSendLoseMessage(CurrentPlayer playerInvader)
    {
        SendLoseMessage(playerInvader);
    }

    [ClientRpc (includeOwner = true)]
    public void RpcSendLoseMessage(CurrentPlayer playerInvader)
    {
        if (LeanLocalization.GetFirstCurrentLanguage() == "Russian") AllSingleton.Instance.chatWindow.AddSystemMessage("Игрок " + playerInvader.playerName + " захватил игрока " + playerName + "!", 1);
        else AllSingleton.Instance.chatWindow.AddSystemMessage("Player " + playerInvader.playerName + " captured the player " + playerName + "!", 1);
    }
    
    [Server]
    public void HomePlanetAddingToPlayer()
    {
        var listPlanet = AllSingleton.Instance.mainPlanetController.listPlanet;
        
        //домашняя планета
        if (listPlanet.Count <= 0) return;
        
        var homePlanet = listPlanet.Find(planet => !planet.isHomePlanet);
        homePlanet.SetHomePlanet();
            
        ChangeListWithPlanets(homePlanet, true);
        homePlanet.Colonization();
    }
    [Command]
    public void CmdHomePlanetAddingToPlayer()
    {
        HomePlanetAddingToPlayer();
    }

    #region Chat

    public static event Action<CurrentPlayer, string> OnMessage;

    [Command]
    public void CmdSend(string message)
    {
        if (message.Trim() != "")
            RpcReceive(message.Trim());
    }

    [ClientRpc]
    public void RpcReceive(string message)
    {
        OnMessage?.Invoke(this, message);
    }

    #endregion
    
    #region Other

    [Server]
    public void SetPlayerData(RoomPlayer roomPlayer)
    {
        playerColor = roomPlayer.playerColor;
        playerName = roomPlayer.playerName;
        indexInvaderSprite = roomPlayer.indexInvaderSprite;
    }
    [Command]
    public void CmdSetPlayerData(RoomPlayer roomPlayer)
    {
        SetPlayerData(roomPlayer);
    }

    [Client]
    public void CameraToHome()
    {
        var position = PlayerPlanets[0].transform.position;
        if (isOwned && NetworkClient.connection.identity.GetComponent<CurrentPlayer>() == this)
            AllSingleton.Instance.cameraController.DoMove(position.x, position.y, 1f);
    }
    #endregion
}
