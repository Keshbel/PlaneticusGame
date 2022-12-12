using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [Header("Screen")] 
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] _resolutions;
    public Toggle fullscreenToggle;

    [Header("Quality")]
    public TMP_Dropdown qualityDropdown;
    
    [Header("Music")]
    public AudioMixer musicMixer;
    public Slider musicSlider; 
    
    [Header("Sound")]
    public AudioMixer soundMixer;
    public Slider soundSlider; 
    
    // Start is called before the first frame update
    void Start()
    {
        //screen resolutions
        resolutionDropdown.onValueChanged.AddListener(UpdateResolution);
        _resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (var index = 0; index < _resolutions.Length; index++)
        {
            var resolution = _resolutions[index];
            string option = resolution.width + " x " + resolution.height;
            options.Add(option);

            if (resolution.width == Screen.currentResolution.width && 
                resolution.height == Screen.currentResolution.height) currentResolutionIndex = index;
        }

        resolutionDropdown.AddOptions(options);
        var resolutionValue = PlayerPrefs.GetInt("Resolution", 0);
        resolutionDropdown.value = resolutionValue == 0 ? currentResolutionIndex : resolutionValue;
        resolutionDropdown.RefreshShownValue();
        
        //fullscreen
        fullscreenToggle.onValueChanged.AddListener(UpdateFullscreen);
        fullscreenToggle.isOn = PlayerPrefsExtra.GetBool("IsFullscreen", true);
        
        //quality
        qualityDropdown.onValueChanged.AddListener(UpdateQuality);
        qualityDropdown.value = PlayerPrefs.GetInt("QualityIndex", 1);
        qualityDropdown.RefreshShownValue();
        
        //audio
        musicSlider.onValueChanged.AddListener(UpdateMusicVolume);
        soundSlider.onValueChanged.AddListener(UpdateSoundVolume);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0);
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 0);
    }

    private void UpdateResolution(int resolutionIndex)
    {
        Resolution resolution = _resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }
    
    private void UpdateFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefsExtra.SetBool("IsFullscreen", isFullscreen);
    }
    
    private void UpdateQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityIndex", qualityIndex);
    }
    
    private void UpdateMusicVolume(float volume)
    {
        musicMixer.SetFloat("VolumeValue", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    private void UpdateSoundVolume(float volume)
    {
        soundMixer.SetFloat("VolumeValue", volume);
        PlayerPrefs.SetFloat("SoundVolume", volume);
    }
}
