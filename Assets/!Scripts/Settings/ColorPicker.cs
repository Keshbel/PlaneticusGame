using System.Collections;
using Mirror.Examples.SyncDir;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
 
public class ColorPicker : MonoBehaviour
{
    public SliderArea SatValSlider;
    public Slider HueSlider;
    public Image DisplayColor;
    public Image SVDisplayColor;
    public static Color Color = Color.clear;
    IEnumerator coroutine;
 
    public UnityEvent onValueChanged;
 
    // Start is called before the first frame update
    void Start()
    {
        if (onValueChanged == null)
            onValueChanged = new UnityEvent();

        HueSlider.value = PlayerPrefs.GetFloat("HueSliderValue", HueSlider.value);

        UpdateColor();
    }
 
    
    void UpdateColor()
    {
        Vector2 satValValue;
        float hue;
        float saturation;
        float value;
         
 
        satValValue = SatValSlider.Value();
        hue = HueSlider.value;
        saturation = satValValue.x;
        value = satValValue.y;
 
        Color prevColor = Color;
        
        Color = Color.HSVToRGB(hue, saturation, value);
 
        Color currentColor = Color;
 
        bool valueChange = currentColor.r != prevColor.r || currentColor.g != prevColor.g || currentColor.b != prevColor.b;
        if (valueChange)
        {
            onValueChanged.Invoke();
            DisplayColor.color = Color;
            SVDisplayColor.color = Color.HSVToRGB(hue, 1, 1);
            PlayerPrefs.SetFloat("HueSliderValue", HueSlider.value);
            PlayerPrefs.Save();
        }
    }
 
    void OnEnable()
    {
        SatValSlider.onValueChanged.AddListener(delegate { UpdateColor(); });
        HueSlider.onValueChanged.AddListener(delegate { UpdateColor(); });
    }
 
 
    void OnDisable()
    {
        SatValSlider.onValueChanged.RemoveAllListeners();
        HueSlider.onValueChanged.RemoveAllListeners();
    }
}