using System.Collections.Generic;
using JamesFrowen.MirrorExamples;
using UnityEngine;

public class ResourceSingleton : MonoBehaviour
{
    [Header("LogisticElements")] 
    public PrefabPoolManager arrowPoolManager;

    [Header("FiveElements")]
    public Sprite earthSprite;
    public Sprite waterSprite;
    public Sprite windSprite;
    public Sprite fireSprite;
    public Sprite aetherSprite;

    [Header("SpaceInvader")] 
    public List<Sprite> invaderSprites;
    
    [Header("Audio")] 
    public AudioSource audioLampOn;
    
    #region Singleton

    public static ResourceSingleton Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    #endregion
}
