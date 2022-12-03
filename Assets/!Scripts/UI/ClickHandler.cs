using System;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ClickHandler : MonoBehaviour
{
    void OnEnable()
    {
        GetComponent<Button>()?.onClick.AddListener(MusicUI.Instance.SoundClickButton);
        GetComponent<Toggle>()?.onValueChanged.AddListener(fakeBool=> MusicUI.Instance.SoundClickButton());
    }

    private void OnDisable()
    {
        GetComponent<Button>()?.onClick.RemoveAllListeners();
        GetComponent<Toggle>()?.onValueChanged.RemoveAllListeners();
    }
}
