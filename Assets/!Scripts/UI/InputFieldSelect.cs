using TMPro;
using UnityEngine;

public class InputFieldSelect : MonoBehaviour
{
    public TMP_InputField inputField;

    private void OnEnable()
    {
        inputField.onSelect.AddListener(DisableCamera);
        inputField.onDeselect.AddListener(EnableCamera);
    }

    private void OnDisable()
    {
        inputField.onSelect.RemoveListener(DisableCamera);
        inputField.onDeselect.RemoveListener(EnableCamera);
    }

    private void EnableCamera(string useless)
    {
        AllSingleton.Instance.cameraController.isEnable = true;
    }

    private void DisableCamera(string useless)
    {
        AllSingleton.Instance.cameraController.isEnable = false;
    }
}
