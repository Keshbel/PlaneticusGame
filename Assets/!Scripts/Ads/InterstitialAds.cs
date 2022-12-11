using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string androidAdID = "Interstitial_Android";
    [SerializeField] private string iOSAdID = "Interstitial_iOS";
    private string _adID;

    #if UNITY_ANDROID || UNITY_IOS
    private void Awake()
    {
        _adID = Application.platform == RuntimePlatform.IPhonePlayer ? iOSAdID : androidAdID;
        LoadAd();
    }
    #endif

    private void LoadAd()
    {
        Debug.Log("Loading Ad: " + _adID);
        Advertisement.Load(_adID, this);
    }

    public void ShowAd()
    {
        Debug.Log("Showing Ad: " + _adID);
        Advertisement.Show(_adID, this);
    }

    public void OnUnityAdsAdLoaded(string placementId) { }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message) { }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message) { }

    public void OnUnityAdsShowStart(string placementId) { }

    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        LoadAd();
    }
}
