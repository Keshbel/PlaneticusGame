using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BackgroundChanger : MonoBehaviour
{
    private static int _indexSprite;

    public List<Sprite> sprites;
    public SpriteRenderer spriteRenderer;
    public TMP_Dropdown dropdown;

    void Start()
    {
        _indexSprite = PlayerPrefs.GetInt("_indexSpriteBackground", 0);
        UpdateBackground();

        if (!dropdown) return;
        dropdown.onValueChanged.AddListener(SetBackground);
        dropdown.value = _indexSprite;
    }

    private void SetBackground(int index)
    {
        _indexSprite = index;
        UpdateBackground();
    }
    
    private void UpdateBackground()
    {
        spriteRenderer.sprite = sprites[_indexSprite];
        PlayerPrefs.SetInt("_indexSpriteBackground", _indexSprite);
        PlayerPrefs.Save();
    }
}
