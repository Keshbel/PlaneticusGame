using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] private string androidGameID = "5055735";
    [SerializeField] private string iOSGameID = "5055734";
    [SerializeField] private bool testMode = true;
    private string _gameID;

    #if UNITY_ANDROID || UNITY_IOS
    private void Awake()
    {
        InitializeAds();
    }
    #endif

    private void InitializeAds()
    {
        _gameID = Application.platform == RuntimePlatform.IPhonePlayer ? iOSGameID : androidGameID;
        Advertisement.Initialize(_gameID, testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}

