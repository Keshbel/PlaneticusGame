using TMPro;
using UnityEngine;

public class InvaderSkinChanger : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    public int indexSprite;

    private void Awake()
    {
        indexSprite = PlayerPrefs.GetInt("indexInvaderSprite", 0);
        dropdown.onValueChanged.AddListener(ChangeSkin);
        dropdown.value = indexSprite;
        dropdown.onValueChanged.AddListener(value => MusicUI.Instance.SoundDropdown());
        
        //dropdown.options[0].
    }

    private void ChangeSkin(int value)
    {
        indexSprite = value;
        PlayerPrefs.SetInt("indexInvaderSprite", indexSprite);
    }
}
