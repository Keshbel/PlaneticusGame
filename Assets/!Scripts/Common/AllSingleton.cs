using System.Collections.Generic;
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
    public CameraMove cameraMove;
    public MainPlanetController mainPlanetController;

    [Header("Panels")]
    public PanelController planetPanelController;
    public PlanetPanelUI planetPanelUI;

    [Header("?")] 
    public GUISkin skin;

    public override void OnStartServer()
    {
        base.OnStartServer();
        
        selectablePlanets = new List<SelectablePlanet>();
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
