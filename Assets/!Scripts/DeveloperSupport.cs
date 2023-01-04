using UnityEngine;

public class DeveloperSupport : MonoBehaviour
{
    public string url;

    public void OpenUrl()
    {
        Application.OpenURL(url);
    }
}
