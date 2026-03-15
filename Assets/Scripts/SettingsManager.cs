using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio")]
    public AudioMixer mainMixer;
    
    public float masterVolume = 0.75f;
    public float bgmVolume = 0.75f;
    public float sfxVolume = 0.75f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        if (mainMixer != null) 
            mainMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        SaveSettings();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = value;
        if (mainMixer != null) 
            mainMixer.SetFloat("BGMVol", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        SaveSettings();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        if (mainMixer != null) 
            mainMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        SaveSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        
        // Initial apply (happens after Awake, usually in Start or via UI initialization)
    }

    public void ApplyAllVolumes()
    {
        SetMasterVolume(masterVolume);
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
    }
}
