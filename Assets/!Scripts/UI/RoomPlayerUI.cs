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
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite notReadySprite;

    public void ChangeReadyIcon(bool oldReadyState, bool newReadyState)
    {
        if (readyImage) readyImage.sprite = newReadyState ? readySprite : notReadySprite;
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
