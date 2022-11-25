using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
 
public class ColorPicker : MonoBehaviour
{
    private RoomManager _roomManager;
    
    public SliderArea satValSlider;
    public Slider hueSlider;
    public Image displayColor;
    public Image svDisplayColor;
    public static Color Color = Color.clear;
    IEnumerator _coroutine;
 
    public UnityEvent onValueChanged;

    private void Awake()
    {
        hueSlider.value = PlayerPrefs.GetFloat("HueSliderValue", hueSlider.value);
        if (!_roomManager) _roomManager = FindObjectOfType<RoomManager>();
    }

    void Start()
    {
        if (onValueChanged == null) onValueChanged = new UnityEvent();

        UpdateColor();
    }
 
    
    void UpdateColor()
    {
        Vector2 satValValue;
        float hue;
        float saturation;
        float value;

        satValValue = satValSlider.Value();
        hue = hueSlider.value;
        saturation = satValValue.x;
        value = satValValue.y;
 
        Color prevColor = Color;
        
        Color = Color.HSVToRGB(hue, saturation, value);
 
        Color currentColor = Color;
 
        bool valueChange = currentColor.r != prevColor.r || currentColor.g != prevColor.g || currentColor.b != prevColor.b;
        if (valueChange)
        {
            onValueChanged.Invoke();
            
            displayColor.color = Color;
            svDisplayColor.color = Color.HSVToRGB(hue, 1, 1);
            _roomManager.playerColor = Color;
            PlayerPrefs.SetFloat("HueSliderValue", hueSlider.value);
            PlayerPrefs.Save();
        }
    }
 
    void OnEnable()
    {
        satValSlider.onValueChanged.AddListener(delegate { UpdateColor(); });
        hueSlider.onValueChanged.AddListener(delegate { UpdateColor(); });
    }

    void OnDisable()
    {
        satValSlider.onValueChanged.RemoveAllListeners();
        hueSlider.onValueChanged.RemoveAllListeners();
    }
}