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
    public GameObject gemShopPanel; // Gem Shop panel inside pause menu

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
        if (gemShopPanel != null) gemShopPanel.SetActive(false);

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
            else if (gemShopPanel != null && gemShopPanel.activeSelf)
            {
                CloseGemShopPanel();
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
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonPress();
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gemShopPanel != null) gemShopPanel.SetActive(false);

        // Check if any minigame is active
        bool inMinigame = false;
        if (PackingMinigameUI.Instance != null && PackingMinigameUI.Instance.windowRoot.activeSelf) inMinigame = true;
        if (KneadingMinigameUI.Instance != null && KneadingMinigameUI.Instance.windowRoot.activeSelf) inMinigame = true;
        if (ShapingMinigameUI.Instance != null && ShapingMinigameUI.Instance.windowRoot.activeSelf) inMinigame = true;

        if (inMinigame)
        {
            Time.timeScale = 0f; // Keep time frozen for minigame
            isPaused = false;
            Cursor.lockState = CursorLockMode.None; // Ensure cursor is free for minigame
            Cursor.visible = true;
            return;
        }

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
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonPress();
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OpenSettings()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonPress();
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonPress();
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

    // ─── Gem Shop ────────────────────────────────────────────────────

    public void OpenGemShopPanel()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonPress();
        if (gemShopPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            gemShopPanel.SetActive(true);
        }
        else if (GemManager.Instance != null)
        {
            // Fallback: use GemManager's panel if it's a separate world-space panel
            GemManager.Instance.OpenGemShop();
        }
    }

    public void CloseGemShopPanel()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonPress();
        if (gemShopPanel != null)
        {
            gemShopPanel.SetActive(false);
            pauseMenuPanel.SetActive(true);
        }
        else if (GemManager.Instance != null)
        {
            GemManager.Instance.CloseGemShop();
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
