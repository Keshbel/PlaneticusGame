using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerUI : NetworkBehaviour
{
    [SyncVar (hook = nameof(UpdateNickname))] public string nickname;
    public TMP_Text nicknameTMP; 
    public Button readyButton; 
    public Button leaveButton;

    [SerializeField] private Image readyImage;

    public void ChangeReadyIcon(bool oldReadyState, bool newReadyState)
    {
        if (readyImage) readyImage.color = newReadyState ? Color.green : Color.white;
    }
    
    [Server]
    public void SetNickname(string value)
    {
        nickname = value;
    }
    [Command (requiresAuthority = false)]
    public void CmdSetNickname(string value)
    {
        SetNickname(value);
    }

    public void UpdateNickname(string oldString, string newString)
    {
        nicknameTMP.text = newString;
    }
}
