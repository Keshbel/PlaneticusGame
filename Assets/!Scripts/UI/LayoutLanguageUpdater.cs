using Lean.Localization;
using UnityEngine;
using UnityEngine.UI;

public class LayoutLanguageUpdater : MonoBehaviour
{
    public HorizontalLayoutGroup horizontalLayoutGroup;

    private void OnEnable()
    {
        if (!horizontalLayoutGroup) horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        LeanLocalization.OnLocalizationChanged += UpdateLanguage;
        
        UpdateLanguage();
    }

    private void OnDisable()
    {
        LeanLocalization.OnLocalizationChanged -= UpdateLanguage;
    }

    private void UpdateLanguage()
    {
        Invoke(nameof(UpdateLanguageFunction), 0.03f);
    }
    
    private void UpdateLanguageFunction()
    {
        horizontalLayoutGroup.childScaleWidth = !horizontalLayoutGroup.childScaleWidth;
    }
}
