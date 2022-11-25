using TMPro;
using UnityEngine;

public class PlayerNameChanger : MonoBehaviour
{
    private RoomManager _roomManager;
    
    public string playerName = "Player";
    public TMP_InputField playerNameText;

    private void Awake()
    {
        if (!_roomManager) _roomManager = FindObjectOfType<RoomManager>();
    }

    private void Start()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "Player");
        playerNameText.text = playerName;
        _roomManager.playerName = playerName;
        playerNameText.onEndEdit.AddListener(SetPlayerName);
    }

    private void SetPlayerName(string pName)
    {
        playerName = pName;
        _roomManager.playerName = playerName;
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();
    }
}
