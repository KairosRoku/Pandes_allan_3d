using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Time Settings")]
    public float realSecondsPerGameHour = 60f; // 1 game hour = 60 real seconds by default
    public int startHour = 3; // 3 AM
    public int endHour = 12;  // 12 PM
    public int serviceStartHour = 5; // Customers start at 5 AM

    private float gameTimeTimer = 0f;
    public bool isDayActive = false;

    [Header("Economy")]
    public int totalMoney = 100;
    public int moneyEarnedToday = 0;
    public int currentDay = 1;

    [Header("End Day UI")]
    public GameObject dayEndWindow;
    public TextMeshProUGUI statsText;

    [Header("HUD")]
    public GameObject hudPanel;
    public GameObject prepPhaseIndicator;
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI dayText; // New day counter HUD text

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartDay();
    }

    public void ToggleHUD(bool active)
    {
        if (hudPanel != null)
            hudPanel.SetActive(active);
    }

    public void StartDay()
    {
        gameTimeTimer = 0f;
        moneyEarnedToday = 0;
        isDayActive = true;
        dayEndWindow.SetActive(false);
        UpdateHUD();

        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isDayActive)
        {
            gameTimeTimer += Time.deltaTime;
            
            UpdateHUD();

            float totalHoursPassed = gameTimeTimer / realSecondsPerGameHour;
            if (startHour + totalHoursPassed >= endHour)
            {
                EndDay();
            }
        }
    }

    private void UpdateHUD()
    {
        float totalHoursPassed = gameTimeTimer / realSecondsPerGameHour;
        float currentDisplayHour = startHour + totalHoursPassed;
        
        int hours = Mathf.FloorToInt(currentDisplayHour);
        int minutes = Mathf.FloorToInt((currentDisplayHour - hours) * 60f);

        if (clockText != null)
            clockText.text = $"{hours:D2}:{minutes:D2} AM";

        if (moneyText != null)
            moneyText.text = $"${totalMoney}";

        if (dayText != null)
            dayText.text = $"Day {currentDay}";

        // Show prep phase indicator only before service time
        if (prepPhaseIndicator != null)
            prepPhaseIndicator.SetActive(!IsServiceTime());
    }

    public bool IsServiceTime()
    {
        float totalHoursPassed = gameTimeTimer / realSecondsPerGameHour;
        return (startHour + totalHoursPassed) >= serviceStartHour;
    }

    public void AddMoney(int amount)
    {
        totalMoney += amount;
        moneyEarnedToday += amount;
        UpdateHUD();
    }

    private void EndDay()
    {
        isDayActive = false;
        dayEndWindow.SetActive(true);

        // Unlock cursor for UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (statsText != null)
        {
            statsText.text = $"DAY {currentDay} COMPLETE\n\n" +
                             $"Money Earned: ${moneyEarnedToday}\n" +
                             $"Total Balance: ${totalMoney}";
        }
    }

    public void NextDay()
    {
        currentDay++;
        StartDay();
    }

    public void BuyItem(ItemType type, int cost)
    {
        if (totalMoney >= cost)
        {
            totalMoney -= cost;
            UpdateHUD();
            Debug.Log("Bought " + type + " for " + cost);
        }
    }
}
