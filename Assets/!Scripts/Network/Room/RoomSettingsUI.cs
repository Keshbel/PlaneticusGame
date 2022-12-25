using Mirror;
using TMPro;

public class RoomSettingsUI : NetworkBehaviour
{
    public TMP_InputField botCountInput;

    void Start()
    {
        if (NetworkManager.singleton.mode != NetworkManagerMode.Host)
        {
            botCountInput.interactable = false;
        }
        
        botCountInput.onEndEdit.AddListener(ChangeBotCount);
    }

    private void ChangeBotCount(string count)
    {
        RoomSettings.Instance.CmdSetBotCount(int.Parse(count));
    }

    #region Singletone

    public static RoomSettingsUI Instance;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    #endregion
}
