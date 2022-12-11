using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Music")]
    public AudioMixer musicMixer;
    public Slider musicSlider; 
    
    [Header("Sound")]
    public AudioMixer soundMixer;
    public Slider soundSlider; 
    
    // Start is called before the first frame update
    void Start()
    {
        musicSlider.onValueChanged.AddListener(UpdateMusicVolume);
        soundSlider.onValueChanged.AddListener(UpdateSoundVolume);

        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0);
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 0);
    }

    private void UpdateMusicVolume(float value)
    {
        musicMixer.SetFloat("VolumeValue", value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
    
    private void UpdateSoundVolume(float value)
    {
        soundMixer.SetFloat("VolumeValue", value);
        PlayerPrefs.SetFloat("SoundVolume", value);
    }
}
