using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuUI : MonoBehaviour
{
    public static bool isPaused = false;

    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Audio Sliders")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Sync sliders with current levels
        if (SettingsManager.Instance != null)
        {
            if (masterSlider != null) masterSlider.value = SettingsManager.Instance.masterVolume;
            if (bgmSlider != null) bgmSlider.value = SettingsManager.Instance.bgmVolume;
            if (sfxSlider != null) sfxSlider.value = SettingsManager.Instance.sfxVolume;
        }
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                if (isPaused) Resume();
                else Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Restore cursor lock if day is active
        if (GameManager.Instance != null && GameManager.Instance.isDayActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Pause()
    {
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void EndDayButton()
    {
        Resume();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndDay();
        }
    }

    // Audio Methods called by Slider OnValueChanged
    public void OnMasterVolumeChanged(float val)
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.SetMasterVolume(val);
    }

    public void OnBGMVolumeChanged(float val)
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.SetBGMVolume(val);
    }

    public void OnSFXVolumeChanged(float val)
    {
        if (SettingsManager.Instance != null) SettingsManager.Instance.SetSFXVolume(val);
    }
}
