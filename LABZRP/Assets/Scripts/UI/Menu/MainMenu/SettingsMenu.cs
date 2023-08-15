using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    
    private bool isFullScreen;
    private int resolutionIndex;
    private float volume;
    private int qualityIndex;
    private bool selectedFullScreen;
    private int selectedResolutionIndex;
    private float selectedVolume;
    private int selectedQualityIndex;
    [SerializeField] Button firstSelectedButton;
    [SerializeField] Toggle fullScreenToggle;
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Dropdown resolutionDropdown;
    [SerializeField] Scrollbar volumeSlider;
    private Resolution[] _resolutions;
    private void Start()
    {
        resolutionDropdown.ClearOptions();
        isFullScreen = Screen.fullScreen;
        _resolutions = Screen.resolutions;
        fullScreenToggle.isOn = isFullScreen;
        selectedFullScreen = isFullScreen;
        qualityIndex = QualitySettings.GetQualityLevel();
        selectedQualityIndex = qualityIndex;
        volume = PlayerPrefs.GetFloat("volume", 1f);
        selectedVolume = volume;
        volumeSlider.value = volume;
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = _resolutions[i].width + " x " + _resolutions[i].height + " " + _resolutions[i].refreshRate + "Hz";
            options.Add(option);
            if(_resolutions[i].width == Screen.currentResolution.width && _resolutions[i].height == Screen.currentResolution.height && _resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
                currentResolutionIndex = i;
        }
        resolutionIndex = currentResolutionIndex;
        selectedResolutionIndex = resolutionIndex;
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
    

    public void setResolution(int resolutionIndex)
    {
        this.selectedResolutionIndex = resolutionIndex;
    }
    public void SetVolume (float volume)
    {
        this.selectedVolume = volume;
    }
    
    public void SetQuality (int qualityIndex)
    {
        this.selectedQualityIndex = qualityIndex;
    }
    
    public void focusOnFirstButton()
    {
        firstSelectedButton.Select();
    }
    public void SetFullScreen(bool isFullScreen)
    {
        selectedFullScreen = isFullScreen;
    }

    public void returnSettings(string scene)
    {
        switch (scene)
        {
            case "MainMenu":
                MainMenuManager mainMenu = GameObject.FindGameObjectWithTag("MainMenuOptions").GetComponent<MainMenuManager>();
                mainMenu.enableOptions();
                break;
            
            case "PauseMenu":
                PauseMenu pauseMenu = GameObject.FindGameObjectWithTag("PauseMenuOptions").GetComponent<PauseMenu>();
                pauseMenu.closeSettingsOption();
                break;
        }
    }

    public void applySettings(string scene)
    {
        resolutionIndex = selectedResolutionIndex;
        isFullScreen = selectedFullScreen;
        volume = selectedVolume;
        qualityIndex = selectedQualityIndex;
        Resolution resolution = _resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, isFullScreen, resolution.refreshRate);
        audioMixer.SetFloat("Master", selectedVolume);
        QualitySettings.SetQualityLevel(qualityIndex);
        Screen.fullScreen = isFullScreen;
        returnSettings(scene);
    }
}
