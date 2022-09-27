using Mirror;
using UnityEngine;

public class AllSingleton : NetworkBehaviour
{
    public CurrentPlayer currentPlayer;
    public CameraMove cameraMove;
    public MainPlanetController planetGeneration;
    
    [Header("PlanetPanel")]
    public PanelController planetPanelController;
    public PlanetPanelUI planetPanelUI;
    
    #region Singleton

    public static AllSingleton instance;

    private void Awake()
    {
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
