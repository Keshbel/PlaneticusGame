using Mirror;

public class RoomSettings : NetworkBehaviour
{
    [SyncVar (hook = nameof(UpdateBotCount))] public int botCount;

    [Command (requiresAuthority = false)]
    public void CmdSetBotCount(int count)
    {
        botCount = count;
    }

    #region Hooks

    private void UpdateBotCount(int oldInt, int newInt)
    {
        if (RoomSettingsUI.Instance != null) RoomSettingsUI.Instance.botCountInput.text = newInt.ToString();
    }

    #endregion
    
    #region Singletone

    public static RoomSettings Instance;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    #endregion
}
