using System.Collections.Generic;
using Lean.Gui;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [Header("Screen")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    private Resolution[] _resolutions;

    [Header("Quality")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    
    [Header("Music")]
    [SerializeField] private AudioMixer musicMixer;
    [SerializeField] private Slider musicSlider; 
    
    [Header("Sound")]
    [SerializeField] private AudioMixer soundMixer;
    [SerializeField] private Slider soundSlider;

    [Header("Game")] 
    [SerializeField] private LeanToggle pointersToggle;
    
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
        resolutionDropdown.onValueChanged.AddListener(value => MusicUI.Instance.SoundDropdown());
        
        //fullscreen
        fullscreenToggle.onValueChanged.AddListener(UpdateFullscreen);
        fullscreenToggle.isOn = PlayerPrefsExtra.GetBool("IsFullscreen", true);
        
        //quality
        qualityDropdown.onValueChanged.AddListener(UpdateQuality);
        qualityDropdown.value = PlayerPrefs.GetInt("QualityIndex", 1);
        qualityDropdown.RefreshShownValue();
        qualityDropdown.onValueChanged.AddListener(value => MusicUI.Instance.SoundDropdown());
        
        //audio
        musicSlider.onValueChanged.AddListener(UpdateMusicVolume);
        soundSlider.onValueChanged.AddListener(UpdateSoundVolume);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0);
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 0);
        
        //toggle
        pointersToggle.OnOn.AddListener(UpdatePointers);
        pointersToggle.OnOff.AddListener(UpdatePointers);
        pointersToggle.On = PlayerPrefsExtra.GetBool("IsPointers", true);
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

    private void UpdatePointers()
    {
        var isPointers = pointersToggle.On;
        PlayerPrefsExtra.SetBool("IsPointers", isPointers);
        if (PointerManager.Instance != null) PointerManager.Instance.IsOn = isPointers;
    }
}
