using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Light2D light2D;
    public void Awake()
    {
        light2D.m_LightCookieSprite = spriteRenderer.sprite;
    }
}
