using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CanvasGroup))]
public class PanelController : MonoBehaviour
{
    //components
    public bool isOpen;
    public GameObject panel;
    [FormerlySerializedAs("canvasGroup")] public CanvasGroup darknessGroup;

    //anim parameters
    public float scaleDefault = 1f;
    public float duration = 0.5f;

    //tweens
    public Tweener TweenerFade;
    private Tweener _tweenerScale;

    private void Start()
    {
        /*if (!panel) panel = gameObject;*/
        if (!darknessGroup) darknessGroup = GetComponent<CanvasGroup>();
        if (darknessGroup.alpha > 0.9f) ClosePanel();
    }

    public void OpenPanel()
    {
        MusicUI.Instance.SoundPanelOpen();
        
        ScaleFadeOut();
        if (AllSingleton.Instance != null) AllSingleton.Instance.cameraController.isEnable = false;
        else Camera.main.GetComponent<CameraController>().isEnable = false;
        isOpen = true;
    }

    public void ClosePanel()
    {
        MusicUI.Instance.SoundPanelOpen();
        
        ScaleFadeIn();
        if (AllSingleton.Instance != null) AllSingleton.Instance.cameraController.isEnable = true;
        else Camera.main.GetComponent<CameraController>().isEnable = true;
        isOpen = false;
    }
    
    public void ScaleFadeOut()
    {
        darknessGroup.interactable = true;
        darknessGroup.blocksRaycasts = true;
        
        TweenerFade?.Kill();
        if (darknessGroup) TweenerFade = darknessGroup.DOFade(1, duration);
        
        _tweenerScale?.Kill();
        if (panel) _tweenerScale = panel.transform.DOScale(scaleDefault, duration);
    }
    
    public void ScaleFadeIn()
    {
        TweenerFade?.Kill();
        if (darknessGroup) TweenerFade = darknessGroup.DOFade(0, duration);
        
        _tweenerScale?.Kill();
        if (panel) _tweenerScale = panel.transform.DOScale(0f, duration);
        
        darknessGroup.interactable = false;
        darknessGroup.blocksRaycasts = false;
    }
}
