using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    public string playerName = "Player";
    public Color playerColor;

    private void Awake()
    {
        playerName = PlayerPrefs.GetString("playerName", "Player");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SavePlayerSettings();
    }

    private void OnApplicationQuit()
    {
        SavePlayerSettings();
    }

    private void OnDestroy()
    {
        SavePlayerSettings();
    }

    private void SavePlayerSettings()
    {
        PlayerPrefs.SetString("playerName", playerName);
        //PlayerPrefs.SetString("playerColor", ColorToHex(playerColor));
    }
    
    //pri
}
