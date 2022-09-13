using UnityEngine;

public class ResourceSingleton : MonoBehaviour
{
    public Sprite earthSprite;
    public Sprite waterSprite;
    public Sprite windSprite;
    public Sprite fireSprite;
    public Sprite aetherSprite;
    
    #region Singleton

    public static ResourceSingleton instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    #endregion
}
