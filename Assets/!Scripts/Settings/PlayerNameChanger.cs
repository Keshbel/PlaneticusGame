using TMPro;
using UnityEngine;

public class PlayerNameChanger : MonoBehaviour
{
    public static string PlayerName = "Player";
    public TMP_InputField playerNameText;

    private void Start()
    {
        PlayerName = PlayerPrefs.GetString("PlayerName", "Player");
        playerNameText.text = PlayerName;
        playerNameText.onEndEdit.AddListener(SetPlayerName);
    }

    private void SetPlayerName(string pName)
    {
        PlayerName = pName;
        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.Save();
    }
}
