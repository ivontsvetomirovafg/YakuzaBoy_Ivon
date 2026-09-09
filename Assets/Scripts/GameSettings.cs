using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
public class GameSettings : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Dropdown resolutionDropdown;
    [SerializeField]
    private TMP_Dropdown fpsDropdown;
    [SerializeField]
    private Toggle fullscreenToggle;
    [SerializeField]
    private Slider musicSlider, ambientSlider; 
    private DataSettings dataSettings;

    [SerializeField]
    private AudioClip buttonSFX;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadSettings();
        SetUIElements();
    }
 
    private void LoadSettings()
    {
        if(PlayerPrefs.HasKey("SettingsData") == true)
        {
            string data = PlayerPrefs.GetString("SettingsData");
            dataSettings = JsonUtility.FromJson<DataSettings>(data);
        }
        else
        {
            dataSettings = new DataSettings();
            SetDefaulDatatValues();
        }
    }
    void SetDefaulDatatValues()
    {
        dataSettings.musicVolume = 1.0f;
        dataSettings.ambientVolume = 1.0f;
        dataSettings.fullscreen = true;
        dataSettings.fps = 1;
        Resolution[] resolutions = Screen.resolutions;
        for(int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                dataSettings.resolution = i;
                break;
            }
        }
    }
    private void SaveSettings()
    {
        string data = JsonUtility.ToJson(dataSettings);
        PlayerPrefs.SetString("SettingsData", data);
    }
 
    private void SetUIElements()
    {
        //Sliders sonido
        musicSlider.value = dataSettings.musicVolume;
        ambientSlider.value = dataSettings.ambientVolume;

        //Toggle FullScreen
        fullscreenToggle.isOn = dataSettings.fullscreen;

        //Dropdownfps
        fpsDropdown.value = dataSettings.fps;

        //Dropdown resolutions
        resolutionDropdown.ClearOptions();
        Resolution[] optionsResolutions = Screen.resolutions;
        for(int i = 0; i < optionsResolutions.Length; i++)
        {
            string option = optionsResolutions[i].width.ToString() + "x" + optionsResolutions[i].height.ToString();
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData(option);
            resolutionDropdown.options.Add(optionData);
        }
        resolutionDropdown.value = dataSettings.resolution;
    }
    public void ApplyButton()
    {
        AudioManager.Instance.PlaySFX(buttonSFX);
        //Music volume
        dataSettings.musicVolume = musicSlider.value;
        AudioManager.Instance.SetMusicVolume(dataSettings.musicVolume);

        //ambient volume (SFX)
        dataSettings.ambientVolume = ambientSlider.value;
        AudioManager.Instance.SetSFXVolume(dataSettings.ambientVolume);

        //toggle fullscreen
        dataSettings.fullscreen = fullscreenToggle.isOn;
        Screen.fullScreen = dataSettings.fullscreen;

        //fps
        dataSettings.fps = fpsDropdown.value;
        switch(dataSettings.fps)
        {
            case 0:
                Application.targetFrameRate = 30;
                break;
            case 1:
                Application.targetFrameRate= 60;
                break;
            case 2:
                Application.targetFrameRate = 120;
                break;
            case 3:
                Application.targetFrameRate = -1;
                break;
        }

        //Resolution
        dataSettings.resolution = resolutionDropdown.value;
        Resolution resolutions = Screen.resolutions [dataSettings.resolution];
        Screen.SetResolution(resolutions.width, resolutions.height, dataSettings.fullscreen);
 
        SaveSettings();
    }
}
 
[System.Serializable]
public class DataSettings
{
    public float musicVolume;
    public float ambientVolume;
    public bool fullscreen;
    public int fps;
    public int resolution;
}
