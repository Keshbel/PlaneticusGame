using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Light2D light2D;
    private readonly FieldInfo _lightCookieSprite =  typeof( Light2D ).GetField( "m_LightCookieSprite", BindingFlags.NonPublic | BindingFlags.Instance );

    public void Awake()
    {
        UpdateCookieSprite(spriteRenderer.sprite);
    }
    
    void UpdateCookieSprite(Sprite sprite) //приходится выкручиваться, получая доступ к sprite field
    {
        _lightCookieSprite.SetValue(light2D, sprite);
    }
}
