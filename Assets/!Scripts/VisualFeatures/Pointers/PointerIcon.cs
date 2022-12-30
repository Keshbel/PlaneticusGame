using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PointerIcon : MonoBehaviour 
{
    public Image arrowImage;

    [Header("Home")]
    public bool isHomeIcon;
    public GameObject homeIcon;
    
    private bool _isShown = true;

    private void Awake() 
    {
        arrowImage.enabled = false;
        _isShown = false;
    }

    public void SetIconPosition(Vector3 position, Quaternion rotation) 
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    public void Show() 
    {
        if (_isShown) return;
        _isShown = true;
        StopAllCoroutines();
        StartCoroutine(ShowProcess());
    }

    public void Hide() 
    {
        if (!_isShown) return;
        _isShown = false;

        StopAllCoroutines();
        StartCoroutine(HideProcess());
    }

    IEnumerator ShowProcess()
    {
        arrowImage.enabled = true;
        if (isHomeIcon) homeIcon.SetActive(true);
        transform.localScale = Vector3.zero;
        
        for (float t = 0; t < 1f; t += Time.deltaTime * 4f)
        {
            transform.localScale = Vector3.one * t;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    IEnumerator HideProcess()
    {
        for (float t = 0; t < 1f; t += Time.deltaTime * 4f)
        {
            transform.localScale = Vector3.one * (1f - t);
            yield return null;
        }

        if (isHomeIcon) homeIcon.SetActive(false);
        arrowImage.enabled = false;
    }
}
