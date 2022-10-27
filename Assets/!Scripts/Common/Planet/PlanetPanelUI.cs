using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetPanelUI : MonoBehaviour
{
    public TMP_Text planetNameText;

    [Header("Toggles and Buttons")] 
    public List<Toggle> resToggles;
    
    public Toggle waterToggle;
    public Toggle earthToggle;
    public Toggle fireToggle;
    public Toggle airToggle;
    public Toggle aetherToggle;
    
    public Button logisticButton;

    [Header("Images and Texts")]
    public Image resourceIconImage;
    public TMP_Text resourceNameText;
    public TMP_Text resourceDeliveryText;
    public TMP_Text resourceMiningText;
    public TMP_Text resourceAllText;
}
