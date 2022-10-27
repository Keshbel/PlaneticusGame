using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogisticRouteUI : MonoBehaviour
{
    public PanelController panel;
    
    public TMP_Text planetNameText;

    [Header("Toggles and Buttons")] 
    public List<Button> resButtons;
    
    [Space] //water
    public Button waterButton;
    public TMP_Text waterText;
    
    [Space] //earth
    public Button earthButton;
    public TMP_Text earthText;
    
    [Space] //fire
    public Button fireButton;
    public TMP_Text fireText;
    
    [Space]//air
    public Button airButton;
    public TMP_Text airText;
    
    [Space] //aether
    public Button aetherButton;
    public TMP_Text aetherText;
    
    [Space] //deleteAll
    public Button deleteRouteButton;
}
