using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
#if UNITY_ANDROID || UNITY_IOS
    [SerializeField] private Button buttonShowAd;
    
    [SerializeField] private string androidAdID = "Rewarded_Android";
    [SerializeField] private string iOSAdID = "Rewarded_iOS";
    private string _adID;

    
    private void Awake()
    {
        _adID = Application.platform == RuntimePlatform.IPhonePlayer ? iOSAdID : androidAdID;

        if (buttonShowAd) buttonShowAd.interactable = false;
    }

    private void Start()
    {
        LoadAd();
    }

    private void OnDestroy()
    {
        if (buttonShowAd) buttonShowAd.onClick.RemoveAllListeners();
    }

    public void LoadAd()
    {
        Debug.Log("Loading Ad: " + _adID);
        Advertisement.Load(_adID, this);
    }

    public void ShowAd()
    {
        Debug.Log("Showing Ad: " + _adID);
        Advertisement.Show(_adID, this);
    }

#endif

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Ad Loaded: " + placementId);

#if UNITY_ANDROID || UNITY_IOS
        if (placementId.Equals(_adID) && buttonShowAd)
        {
            buttonShowAd.onClick.AddListener(ShowAd);
            buttonShowAd.interactable = true;
        }
#endif
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message) { }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message) { }

    public void OnUnityAdsShowStart(string placementId) { }

    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (placementId.Equals(_adID) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
        }
#endif
    }
}
