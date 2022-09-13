using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetPanelUI : MonoBehaviour
{
    public TMP_Text planetNameText;

    [Header("Toggles and Buttons")] 
    public List<Toggle> resToggles;
    public Button deliveryButton;
    
    /*public Toggle resToggle1;
    public Toggle resToggle2;
    public Toggle resToggle3;
    public Toggle resToggle4;
    public Toggle resToggle5;*/
    
    [Header("Images and Texts")]
    public Image resourceIconImage;
    public TMP_Text resourceNameText;
    public TMP_Text resourceDeliveryText;
    public TMP_Text resourceMiningText;
    public TMP_Text resourceAllText;
}
