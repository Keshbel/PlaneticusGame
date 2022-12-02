using System.Collections.Generic;
using ChatForStrategy;
using JamesFrowen.MirrorExamples;
using Mirror;
using UnityEngine;

public class AllSingleton : NetworkBehaviour
{
    [Header("Players")]
    public CurrentPlayer player;
    public readonly SyncList<CurrentPlayer> Players = new SyncList<CurrentPlayer>();
    public List<SelectablePlanet> selectablePlanets;

    [Header("Options")] 
    public float speed = 0.3f;
    
    [Header("Prefabs/Pool")] 
    public GameObject planetPrefab;
    public PrefabPoolManager invaderPoolManager;
    public GameObject selectUnitsPrefab;

    [Header("Scripts")]
    public CameraController cameraController;
    public MainPlanetController mainPlanetController;
    public ChatWindow chatWindow;

    [Header("Panels")]
    public PanelController planetPanelController;
    public PlanetPanelUI planetPanelUI;
    [Space] public EndGame endGame;

    [Header("?")] 
    public GUISkin skin;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        selectablePlanets = new List<SelectablePlanet>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!cameraController) cameraController = Camera.main.GetComponent<CameraController>();
    }

    #region Singleton
    
    public static AllSingleton Instance;
    
    private void Awake()
    {
        if (!mainPlanetController)
            mainPlanetController = FindObjectOfType<MainPlanetController>();
        
        if (Instance != null) NetworkServer.Destroy(gameObject);
        else Instance = this;
    }
    #endregion
}
