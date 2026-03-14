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

    [Header("Shop Settings")]
    public int flourRestockCost = 10;
    public int sugarRestockCost = 10;
    public int waterRestockCost = 5;
    public int restockAmountPerPurchase = 5;

    [Header("Shop UI Texts")]
    public TextMeshProUGUI shopFlourAmountText;
    public TextMeshProUGUI shopSugarAmountText;
    public TextMeshProUGUI shopWaterAmountText;
    public TextMeshProUGUI shopMoneyText; // Added to show money in the shop menu

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
        return GetCurrentHour() >= serviceStartHour;
    }

    public float GetCurrentHour()
    {
        float totalHoursPassed = gameTimeTimer / realSecondsPerGameHour;
        return startHour + totalHoursPassed;
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

        UpdateShopAmountsUI();
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

    public void BuyFlour()
    {
        TryRestock(ItemType.Flour, flourRestockCost);
    }

    public void BuySugar()
    {
        TryRestock(ItemType.Sugar, sugarRestockCost);
    }

    public void BuyWater()
    {
        TryRestock(ItemType.Water, waterRestockCost);
    }

    private void TryRestock(ItemType type, int cost)
    {
        if (totalMoney >= cost)
        {
            totalMoney -= cost;
            UpdateHUD();
            
            // Refill all dispensers for this type
            Dispenser[] dispensers = FindObjectsOfType<Dispenser>();
            foreach (var d in dispensers)
            {
                if (d.itemType == type) d.Restock(restockAmountPerPurchase);
            }

            // Refill all ingredient racks for this type
            IngredientRack[] racks = FindObjectsOfType<IngredientRack>();
            foreach (var r in racks)
            {
                if (r.itemType == type) r.Restock(restockAmountPerPurchase);
            }

            Debug.Log($"[SHOP] Bought {restockAmountPerPurchase} {type} for ${cost}.");
            UpdateShopAmountsUI();
        }
        else
        {
            Debug.Log($"[SHOP] Not enough money to buy {type}!");
        }
    }

    public void UpdateShopAmountsUI()
    {
        if (shopMoneyText != null)
            shopMoneyText.text = $"Balance: ${totalMoney}";

        if (shopFlourAmountText != null)
            shopFlourAmountText.text = $"Owned: {GetTotalIngredientAmount(ItemType.Flour)}";
            
        if (shopSugarAmountText != null)
            shopSugarAmountText.text = $"Owned: {GetTotalIngredientAmount(ItemType.Sugar)}";
            
        if (shopWaterAmountText != null)
            shopWaterAmountText.text = $"Owned: {GetTotalIngredientAmount(ItemType.Water)}";
    }

    private int GetTotalIngredientAmount(ItemType type)
    {
        int total = 0;
        Dispenser[] dispensers = FindObjectsOfType<Dispenser>();
        foreach (var d in dispensers)
        {
            if (d.itemType == type) total += d.currentAmount;
        }

        IngredientRack[] racks = FindObjectsOfType<IngredientRack>();
        foreach (var r in racks)
        {
            if (r.itemType == type) total += r.currentAmount;
        }

        return total;
    }
}
