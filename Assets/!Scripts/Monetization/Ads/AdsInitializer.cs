
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
#if UNITY_ANDROID || UNITY_IOS
    [SerializeField] private string androidGameID = "5055735";
    [SerializeField] private string iOSGameID = "5055734";
    [SerializeField] private bool testMode = true;
    private string _gameID;

    
    private void Awake()
    {
        InitializeAds();
    }
    

    private void InitializeAds()
    {
        _gameID = Application.platform == RuntimePlatform.IPhonePlayer ? iOSGameID : androidGameID;
        Advertisement.Initialize(_gameID, testMode, this);
    }

#endif

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}

