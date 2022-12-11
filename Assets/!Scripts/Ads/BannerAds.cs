using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAds : MonoBehaviour
{
    [SerializeField] private BannerPosition bannerPosition;
    
    [SerializeField] private string androidAdID = "Banner_Android";
    [SerializeField] private string iOSAdID = "Banner_iOS";
    private string _adID;
  
    #if UNITY_ANDROID || UNITY_IOS
    private void Awake()
    {
        _adID = Application.platform == RuntimePlatform.IPhonePlayer ? iOSAdID : androidAdID;
    }

    private void Start()
    {
        Advertisement.Banner.SetPosition(bannerPosition);
        StartCoroutine(LoadAdBanner());
    }
    #endif

    private IEnumerator LoadAdBanner()
    {
        yield return new WaitForSeconds(1f);
        LoadBanner();
    }

    private void LoadBanner()
    {
        BannerLoadOptions options = new BannerLoadOptions()
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };
        
        Advertisement.Banner.Load(_adID, options);
    }
    
    private void ShowBannerAd()
    {
        BannerOptions options = new BannerOptions()
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };
        
        Advertisement.Banner.Show(_adID, options);
    }
    
    private void OnBannerLoaded()
    {
        Debug.Log("Banner loaded");
        ShowBannerAd();
    }

    private void OnBannerError(string message)
    {
        Debug.LogError($"Banner Error: {message}");
    }

    private void OnBannerClicked() { }
    
    private void OnBannerHidden() { }
    
    private void OnBannerShown() { }
}
