using System.Collections.Generic;
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
    public AudioClip dropdown;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public void SoundClickButton()
    {
        PlayAudio(buttonClick);
    }

    public void SoundPanelOpen()
    {
        PlayAudio(panelOpen);
    }
    
    public void SoundDropdown()
    {
        PlayAudio(dropdown);
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
