using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class AllSingleton : NetworkBehaviour
{
    [Header("Players")]
    public CurrentPlayer player;
    public SyncList<NetworkIdentity> syncCurrentPlayers = new SyncList<NetworkIdentity>();
    public List<NetworkIdentity> currentPlayers;
    public List<SelectablePlanet> selectablePlanets;

    [Header("Options")] 
    public float speed = 0.2f;
    
    [Header("Prefabs")] 
    public GameObject planetPrefab;
    public GameObject invaderPrefab;
    
    [Header("Scripts")]
    public CameraMove cameraMove;
    public MainPlanetController mainPlanetController;
    public LogisticRouteController logisticRouteController;
    
    [Header("Panels")]
    public PanelController planetPanelController;
    public PlanetPanelUI planetPanelUI;
    public LogisticRouteUI logisticRouteUI;

    #region Singleton

    public static AllSingleton instance;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        selectablePlanets = new List<SelectablePlanet>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        syncCurrentPlayers.Callback += SyncCurrentPlayer; //вместо hook, для SyncList используем подписку на Callback

        currentPlayers = new List<NetworkIdentity>(syncCurrentPlayers.Count); //так как Callback действует только на изменение массива,  
        for (int i = 0; i < syncCurrentPlayers.Count; i++) //а у нас на момент подключения уже могут быть какие-то данные в массиве, нам нужно эти данные внести в локальный массив
        {
            //SyncListPlanet(SyncList<PlanetController>.Operation.OP_ADD, i, planetPrefab.AddComponent<PlanetController>(), syncListPlanet[i]);
            currentPlayers.Add(syncCurrentPlayers[i]);
        }
    }

    [Server]
    public void AddPlayer(NetworkIdentity newPlayer)
    {
        syncCurrentPlayers.Add(newPlayer);
    }
    [Command]
    public void CmdAddPlayer(NetworkIdentity newPlayer)
    {
        AddPlayer(newPlayer);
    }

    private void SyncCurrentPlayer(SyncList<NetworkIdentity>.Operation op, int index, NetworkIdentity oldItem,
        NetworkIdentity newItem) //обработчик ля синхронизации планет
    {
        switch (op)
        {
            case SyncList<NetworkIdentity>.Operation.OP_ADD:
            {
                currentPlayers.Add(newItem);
                break;
            }
            case SyncList<NetworkIdentity>.Operation.OP_CLEAR:
            {

                break;
            }
            case SyncList<NetworkIdentity>.Operation.OP_INSERT:
            {

                break;
            }
            case SyncList<NetworkIdentity>.Operation.OP_REMOVEAT:
            {

                break;
            }
            case SyncList<NetworkIdentity>.Operation.OP_SET:
            {

                break;
            }
        }
    }

    private void Awake()
    {
        if (!mainPlanetController)
            mainPlanetController = FindObjectOfType<MainPlanetController>();
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    #endregion
}
