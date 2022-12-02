using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    public PanelController panelController;

    [Header("Buttons")] 
    public Button continueButton;
    public Button settingsButton;
    public Button roomButton;
    public Button exitButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(panelController.ClosePanel);
        
        //settingsButton
        
        /*if (NetworkServer.active && NetworkManager.IsSceneActive(RoomManager.Instance.GameplayScene)) roomButton.onClick.AddListener(HostReturnToRoom);
        else */
        roomButton.gameObject.SetActive(false);

        if (NetworkManager.singleton.mode == NetworkManagerMode.Host) exitButton.onClick.AddListener(NetworkManager.singleton.StopHost);
        else exitButton.onClick.AddListener(Utils.Disconnect);
    }

    private void Update()
    {
        if (!Input.GetButtonDown("Cancel")) return;
        
        if (panelController.isOpen) panelController.ClosePanel();
        else panelController.OpenPanel();
    }

    private void HostReturnToRoom()
    {
        if (NetworkServer.active && NetworkManager.IsSceneActive(RoomManager.Instance.GameplayScene))
        {
            NetworkManager.singleton.ServerChangeScene(RoomManager.Instance.RoomScene);
        }
    }
}
