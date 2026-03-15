using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject saveSlotsPanel;

    [Header("Save Slot UI")]
    public TextMeshProUGUI[] slotDescTexts; // Assign 3 texts for Slot 1, 2, 3

    [Header("Audio Sliders")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Scene Settings")]
    public string gameSceneName = "GameScene";

    private void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);

        // Sync sliders with current levels
        if (SettingsManager.Instance != null)
        {
            if (masterSlider != null) masterSlider.value = SettingsManager.Instance.masterVolume;
            if (bgmSlider != null) bgmSlider.value = SettingsManager.Instance.bgmVolume;
            if (sfxSlider != null) sfxSlider.value = SettingsManager.Instance.sfxVolume;
        }

        UpdateSlotUI();

        // Show cursor for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (saveSlotsPanel != null && saveSlotsPanel.activeSelf)
            {
                CloseSaveSlots();
            }
        }
    }

    public void UpdateSlotUI()
    {
        for (int i = 0; i < 3; i++)
        {
            int slot = i + 1;
            if (slotDescTexts.Length > i && slotDescTexts[i] != null)
            {
                if (SaveSystem.DoesSlotExist(slot))
                {
                    SaveData data = SaveSystem.Load(slot);
                    slotDescTexts[i].text = $"Slot {slot}\nDay {data.currentDay} | ${data.totalMoney}";
                }
                else
                {
                    slotDescTexts[i].text = $"Slot {slot}\nEmpty Save";
                }
            }
        }
    }

    public void OpenSaveSlots()
    {
        mainMenuPanel.SetActive(false);
        saveSlotsPanel.SetActive(true);
        UpdateSlotUI();
    }

    public void SelectSlot(int slot)
    {
        SaveSystem.SelectedSlot = slot;
        PlayGame();
    }

    // Helper functions for Unity Buttons
    public void SelectSlot1() => SelectSlot(1);
    public void SelectSlot2() => SelectSlot(2);
    public void SelectSlot3() => SelectSlot(3);

    public void BackToMainMenu()
    {
        CloseSettings();
        CloseSaveSlots();
    }

    public void CloseSaveSlots()
    {
        if (saveSlotsPanel != null) saveSlotsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Application...");
        Application.Quit();
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
