using Mirror;
using UnityEngine;

public static class Utils
{
    public static void Disconnect()
    {
        NetworkManager.singleton.StopClient();
    }
    
    public static void ExitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}
