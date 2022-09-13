using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PanelController : MonoBehaviour
{
    //components
    public GameObject panel;
    public CanvasGroup canvasGroup;

    //anim parameters
    public float scaleDefault;
    public float duration = 0.5f;

    //tweens
    public Tweener TweenerFade;
    private Tweener _tweenerScale;

    private void Start()
    {
        if (!panel)
            panel = gameObject;
        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OpenPanel()
    {
        ScaleFadeOut();
        AllSingleton.instance.cameraMove.isEnable = false;
    }

    public void ClosePanel()
    {
        ScaleFadeIn();
        AllSingleton.instance.cameraMove.isEnable = true;
    }
    
    public void ScaleFadeOut()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        TweenerFade?.Kill();
        TweenerFade = canvasGroup.DOFade(1, duration);
        _tweenerScale?.Kill();
        _tweenerScale = panel.transform.DOScale(scaleDefault, duration);
    }
    
    public void ScaleFadeIn()
    {
        TweenerFade?.Kill();
        TweenerFade = canvasGroup.DOFade(0, duration);
        _tweenerScale?.Kill();
        _tweenerScale = panel.transform.DOScale(0f, duration);
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
