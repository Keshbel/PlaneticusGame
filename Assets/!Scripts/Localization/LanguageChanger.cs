using System.Collections.Generic;
using ChatForStrategy;
using DG.Tweening;
using Lean.Localization;
using UnityEngine;
using UnityEngine.UI;

public class LanguageChanger : MonoBehaviour
{
    public Button buttonChanger;
    public Image languageImage; 
    public List<Sprite> sprites;

    private void Start()
    {
        if (!buttonChanger) buttonChanger = GetComponent<Button>();
        buttonChanger.onClick.AddListener(ChangeLanguage);
        
        SetLanguage(PlayerPrefs.GetInt("Language", 1));
    }

    private void ChangeLanguage()
    {
        switch (LeanLocalization.GetFirstCurrentLanguage())
        {
            case "Russian":
                SetLanguage(1);
                break;
            case "English":
                SetLanguage(0);
                break;
        }
    }

    private void SetLanguage(int languageIndex)
    {
        transform.DOShakePosition(1f, 5);
        languageImage.sprite = sprites[languageIndex];
        LeanLocalization.SetCurrentLanguageAll(languageIndex == 0 ? "Russian" : "English");
        
        PlayerPrefs.SetInt("Language", languageIndex);
        PlayerPrefs.Save();
    }
}
