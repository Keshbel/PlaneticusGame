using System;
using Lean.Localization;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    public PanelController panelController;

    public TMP_Text titleText;
    public TMP_Text descriptionText;
    
    public Button watchButton;
    public Button exitButton;

    private void Awake()
    {
        watchButton.onClick.AddListener(panelController.ClosePanel);
        
        if (NetworkManager.singleton.mode == NetworkManagerMode.Host) exitButton.interactable = false;
        else exitButton.onClick.AddListener(Utils.Disconnect);
    }

    public void VictoryResult()
    {
        print("Победа-победа, время обеда!");
        titleText.text = LeanLocalization.GetFirstCurrentLanguage() == "Russian" ? "Победа" : "Victory";
        descriptionText.text = LeanLocalization.GetFirstCurrentLanguage() == "Russian" ? "Вы победили, поздравляю!" : "You won, congratulations!";
        
        panelController.OpenPanel();
    }
    
    public void DefeatResult()
    {
        print("Вызвалась панель поражения");
        titleText.text = LeanLocalization.GetFirstCurrentLanguage() == "Russian" ? "Поражение" : "Defeat";
        if (NetworkManager.singleton.mode == NetworkManagerMode.Host)
        {
            descriptionText.text = LeanLocalization.GetFirstCurrentLanguage() == "Russian"
                ? "Вы проиграли, со всеми случается..." + Environment.NewLine + "Вам нужно дождаться полного завершения игры! (хост)"
                : "You lose, it happens to everyone..." + Environment.NewLine + "You need to wait until the game is over! (host)";
        }
        else
        {
            descriptionText.text = LeanLocalization.GetFirstCurrentLanguage() == "Russian"
                ? "Вы проиграли, со всеми случается..." + Environment.NewLine + " Можете понаблюдать за текущей игрой или начать следующую!"
                : "You lost, it happens to everyone..." + Environment.NewLine + " You can watch the current game or start the next one!";
        }
        
        panelController.OpenPanel();
    }
}
