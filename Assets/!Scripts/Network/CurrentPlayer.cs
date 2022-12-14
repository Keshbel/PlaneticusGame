using System;
using System.Collections.Generic;
using System.Linq;
using Lean.Localization;
using Mirror;
using UnityEngine;
using Random = System.Random;

[Serializable]
public class CurrentPlayer : NetworkBehaviour
{
    public List<CurrentPlayer> players = new List<CurrentPlayer>();

    [Header("Main")]
    public RoomPlayer roomPlayer;
    [SyncVar] public string playerName;
    [SyncVar] public Color playerColor;
    [SyncVar] public int indexInvaderSprite;
    
    [SyncVar] public SelectUnits selectUnits;
    [SyncVar] public CurrentPlayer enemyPlayerDefeat;

    [Header("SyncLists")]
    public readonly SyncList<PlanetController> PlayerPlanets = new SyncList<PlanetController>();
    
    public readonly SyncList<SpaceInvaderController> PlayerInvaders = new SyncList<SpaceInvaderController>();

    [Header("Bot parameters")]
    [SyncVar] public bool isBot;
    public int SuperPlanetCount => PlayerPlanets.FindAll(planet => planet.isSuperPlanet).Count;
    public List<PlanetController> targetSuperPlanets = new List<PlanetController>();
    [SyncVar] public State currentState;
    public State neutralInvaderState;
    public State nonInvadersState;
    public State aggressiveState;

    [Client]
    private void Update()
    {
        if (!isBot || !currentState) return;

        if (!currentState.IsFinished) currentState.Run();
        else
        {
            var unusedInvaders = PlayerInvaders.Find(invader => invader.MoveTween == null);
            if (SuperPlanetCount >= 3) CmdSetState(unusedInvaders != null ? aggressiveState : nonInvadersState);
            else CmdSetState(unusedInvaders != null ? neutralInvaderState : nonInvadersState);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!isOwned && !isBot) return;

        if (!isBot)
        {
            AllSingleton.Instance.player = this;

            var roomPlayers = FindObjectsOfType<RoomPlayer>().ToList();
            roomPlayer = roomPlayers.Find(p => p.isOwned);
            CmdSetPlayerData(roomPlayer);
        }
        else
        {
            Destroy(selectUnits);
            CmdSetState(neutralInvaderState);
        }

        if (PlayerPlanets.Count == 0) Invoke(nameof(CmdHomePlanetAddingToPlayer), 0.5f);
        Invoke(nameof(AddPlayer), 0.05f);
        Invoke(nameof(CameraToHome), 0.8f);
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
            if (isServer) MainPlanetController.Instance.RemovePlanetFromList(planet.GetComponent<PlanetController>());
            else MainPlanetController.Instance.CmdRemovePlanetFromList(planet.GetComponent<PlanetController>());
        }
    }

    #region Bot

    [Server]
    public void SetState(State state)
    {
        currentState = state;//Instantiate(state);
        //currentState.currentPlayer = this;
        currentState.Init();
    }
    [Command (requiresAuthority = false)]
    public void CmdSetState(State state)
    {
        SetState(state);
    }

    #endregion
    
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
        var planetController = planet.GetComponent<PlanetController>();
        
        if (isAdding)
        {
            PlayerPlanets.Add(planet);
            
            var identity = planet.GetComponent<NetworkIdentity>();
            identity.RemoveClientAuthority();
            identity.AssignClientAuthority(connectionToClient);
            
            planetController.playerOwner = this;
            planetController.colorPlanet = playerColor;
            planetController.Colonization();
        }
        else
        {
            PlayerPlanets.Remove(planet);
            
            planet.GetComponent<NetworkIdentity>().RemoveClientAuthority();
            
            planetController.playerOwner = null;
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
    }
    [Command (requiresAuthority = false)]
    public void CmdChangeListWithInvaders(SpaceInvaderController invader, bool isAdding)
    {
        ChangeListWithInvaders(invader, isAdding);
    }

    [Server]
    public void SpawnInvader(int count, GameObject goPosition) //?????????? ?????????????????????? (?????????? ?????????? ?????????????????? ?? ?????????????????? ?????????????)
    {
        for (int i = 0; i < count; i++)
        {
            var xBoundCollider = goPosition.GetComponent<CircleCollider2D>().bounds.max.x; //?????????????? ???????????????????? ???? ??
            var planetPosition = goPosition.transform.position; //?????????????? ??????????????
            var spawnPosition = new Vector3(xBoundCollider, planetPosition.y, planetPosition.z);

            var invader = AllSingleton.Instance.invaderPoolManager.GetFromPool(spawnPosition, Quaternion.identity);
            if (!isBot) NetworkServer.Spawn(invader, connectionToClient);
            else NetworkServer.Spawn(invader);

            var invaderControllerComponent = invader.GetComponent<SpaceInvaderController>();
            invaderControllerComponent.playerOwner = this;
            invaderControllerComponent.indexInvaderSprite = indexInvaderSprite;
            invaderControllerComponent.SetColor(playerColor);
            invaderControllerComponent.targetTransform = goPosition.transform;
            
            ChangeListWithInvaders(invaderControllerComponent, true);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSpawnInvader(int count, GameObject goPosition)
    {
        SpawnInvader(count, goPosition);
    }
    #endregion

    [Command (requiresAuthority = false)]
    public void CmdDefeat(CurrentPlayer playerInvader)
    {
        RpcDefeat(playerInvader);
    }
    [Client]
    public void Defeat(CurrentPlayer playerInvader)
    {
        print("?? ?????????????? ?????????????????????? (??????????????????)");
        PlayerPlanets.ToList().ForEach(planet => //?????????????????? ?????????????????? ????????????????????
        {
            CmdChangeListWithPlanets(planet,false);
            playerInvader.CmdChangeListWithPlanets(planet,true);
        });
        PlayerInvaders.ToList().ForEach(invader => invader.CmdUnSpawn(invader.gameObject));

        //???????????????????? ?? ??????????????????
        CmdSendLoseMessage(playerInvader);
        //?????????????????? UI ??????????????????
        if (!isBot) AllSingleton.Instance.endGame.DefeatResult();
    }
    [TargetRpc]
    public void RpcDefeat(CurrentPlayer playerInvader)
    {
        Defeat(playerInvader);
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
        if (LeanLocalization.GetFirstCurrentLanguage() == "Russian") AllSingleton.Instance.chatWindow.AddSystemMessage("?????????? " + playerInvader.playerName + " ???????????????? ???????????? " + playerName + "!", 1);
        else AllSingleton.Instance.chatWindow.AddSystemMessage("Player " + playerInvader.playerName + " captured the player " + playerName + "!", 1);
    }

    [Server]
    public void HomePlanetAddingToPlayer()
    {
        var listPlanet = MainPlanetController.Instance.listPlanet;
        var listPlanets = listPlanet.FindAll(planet => !planet.isHomePlanet);

        //???????????????? ??????????????
        if (listPlanet.Count <= 0) return;

        var homePlanet = listPlanet.Find(planet => !planet.isHomePlanet);
        var homePlanetPosition = homePlanet.transform.position;
        var listPlayersPlanets = listPlanet.FindAll(planet => planet.isHomePlanet);
        var distance = MainPlanetController.Instance.xBounds.y / (NetworkServer.connections.Count * 2);
        if (listPlayersPlanets.Count > 0)
            while (true)
            {
                //???????? ???? ?????????????? ?????????????? ???????????? ?? ???????????????? ??????????????????
                var isAnotherPlanetInDistance = listPlayersPlanets.Find(planController =>
                    Vector2.Distance(homePlanetPosition, planController.gameObject.transform.position) < distance
                        ? planController
                        : false);

                if (isAnotherPlanetInDistance)
                {
                    homePlanet = listPlanets[new Random().Next(0, listPlanets.Count())];
                    homePlanetPosition = homePlanet.transform.position;
                }
                else break;
            }

        homePlanet.GetComponent<PlanetController>().playerOwner = this;
        homePlanet.SetHomePlanet();

        ChangeListWithPlanets(homePlanet, true);
        homePlanet.Colonization();
    }

    [Command (requiresAuthority = false)]
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
    public void SetPlayerData(RoomPlayer player)
    {
        playerColor = player.playerColor;
        playerName = player.playerName;
        indexInvaderSprite = player.indexInvaderSprite;
    }
    [Command]
    public void CmdSetPlayerData(RoomPlayer player)
    {
        SetPlayerData(player);
    }

    [Client]
    public void CameraToHome()
    {
        if (isBot || !isOwned || PlayerPlanets.Count == 0 /*&& NetworkClient.connection.identity.GetComponent<CurrentPlayer>() != this*/) return;
        var position = PlayerPlanets[0].transform.position;
        AllSingleton.Instance.cameraController.DoCustomMove(position.x, position.y, 1f);
    }
    #endregion
}
