using System;
using UnityEngine;
using UnityEngine.UI;

public class ColorOutline : MonoBehaviour
{
    public Outline outline;

    private void Start()
    {
        SetOutlineColor();
    }

    public void SetOutlineColor()
    {
        outline.effectColor = ColorPicker.Color;
    }
}
