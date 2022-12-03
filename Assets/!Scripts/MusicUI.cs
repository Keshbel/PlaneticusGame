using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MusicUI : MonoBehaviour
{
    public static MusicUI Instance;

    [Header("Main")]
    public AudioSource audioSource;
    public List<Button> allButtons;

    [Header("Sounds")]
    public AudioClip buttonClick;
    public AudioClip panelOpen;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    
    async void Start()
    {
        /*await Task.Delay(500);
        allButtons = FindObjectsOfType<Button>().ToList();

        await Task.Delay(500);
        foreach (var button in allButtons) button.onClick.AddListener(SoundClickButton);*/
    }

    public void SoundClickButton()
    {
        PlayAudio(buttonClick);
    }

    public void SoundPanelOpen()
    {
        PlayAudio(panelOpen);
    }
    
    public void SoundPanelClose()
    {
        PlayAudio(panelOpen);
    }

    private void PlayAudio(AudioClip audioClip)
    {
        audioSource.pitch = 1;
        audioSource.PlayOneShot(audioClip);
    }

    private void PlayReverseAudio(AudioClip audioClip)
    {
        audioSource.pitch = -1;
        audioSource.timeSamples = audioClip.samples - 1;
        audioSource.PlayOneShot(audioClip);
    }
}
