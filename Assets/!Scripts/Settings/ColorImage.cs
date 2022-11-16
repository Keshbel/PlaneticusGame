using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorImage : MonoBehaviour
{
    public Image image;

    private void Start()
    {
        SetImageColor();
    }

    public void SetImageColor()
    {
        image.color = ColorPicker.Color;
    }
}
