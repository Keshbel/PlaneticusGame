using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class PanelController : MonoBehaviour
{
    //components
    public GameObject panel;
    public CanvasGroup canvasGroup;

    //anim parameters
    public float scaleDefault = 1f;
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
        if (AllSingleton.Instance != null)
            AllSingleton.Instance.cameraMove.isEnable = false;
        else
        {
            Camera.main.GetComponent<CameraMove>().isEnable = false;
        }
    }

    public void ClosePanel()
    {
        ScaleFadeIn();
        if (AllSingleton.Instance != null)
            AllSingleton.Instance.cameraMove.isEnable = true;
        else
        {
            Camera.main.GetComponent<CameraMove>().isEnable = true;
        }
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
