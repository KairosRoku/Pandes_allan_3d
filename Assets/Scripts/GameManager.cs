using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public enum DailyEvent
{
    None,
    Oversleep,
    Bagyo,
    Infestation,
    Vlogger,
    Holiday,
    SchoolEvent,
    Illness
}

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

    [Header("Upgrades")]
    public int doughMakingUpgradeLevel = 0;
    public int bakingUpgradeLevel = 0;
    public int burnTimeUpgradeLevel = 0;

    public int[] doughUpgradeCosts = { 50, 100, 150 };
    public int[] bakingUpgradeCosts = { 50, 100, 150 };
    public int[] burnUpgradeCosts = { 50, 100 };

    [Header("Daily Events")]
    public int dailyCost = 30;
    public DailyEvent currentEvent = DailyEvent.None;
    public TextMeshProUGUI eventHUDText;
    
    [HideInInspector] public bool hasSpawnedVloggerToday = false;
    [HideInInspector] public int viralDaysRemaining = 0;
    [HideInInspector] public int viralFailedDaysRemaining = 0;

    private int illnessCount = 0;
    private int vloggerCount = 0;
    private int infestationCount = 0;
    private int bagyoCount = 0;

    [Header("Event Popups")]
    public GameObject startOfDayWindow;
    public TextMeshProUGUI startOfDayEventText;
    public TextMeshProUGUI endOfDayNewsText;
    
    [Header("Pause UI")]
    public PauseMenuUI pauseMenu;

    [HideInInspector] public DailyEvent nextDayEvent = DailyEvent.None;

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

    [Header("Upgrade UI Texts")]
    public TextMeshProUGUI doughUpgradeText;
    public TextMeshProUGUI bakingUpgradeText;
    public TextMeshProUGUI burnTimeUpgradeText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (pauseMenu == null)
            pauseMenu = FindObjectOfType<PauseMenuUI>();

        nextDayEvent = RollForEvent();
        StartDay();
    }

    public void OpenSettings()
    {
        if (pauseMenu != null)
        {
            pauseMenu.Pause();
            pauseMenu.OpenSettings();
        }
    }

    public void ToggleHUD(bool active)
    {
        if (hudPanel != null)
            hudPanel.SetActive(active);
    }

    public void StartDay()
    {
        if (currentDay == 1 && gameTimeTimer == 0) // First time starting
        {
            LoadGame();
        }

        currentEvent = nextDayEvent;

        gameTimeTimer = 0f;
        moneyEarnedToday = 0;
        dayEndWindow.SetActive(false);

        hasSpawnedVloggerToday = false;

        ApplyEventEffects();

        UpdateHUD();

        if (currentEvent == DailyEvent.Illness)
        {
            totalMoney -= 100; // Medicine cost only, day no longer skipped
            Debug.Log("[EVENT] Illness! Movement speed reduced by 25%. Paid $100 for medicine.");
        }
        else if (currentEvent == DailyEvent.Bagyo)
        {
            Debug.Log("[EVENT] Bagyo! Customers will be extremely rare today.");
        }

        if (startOfDayWindow != null && currentEvent != DailyEvent.None)
        {
            isDayActive = false; // Pause day until OK is clicked
            startOfDayWindow.SetActive(true);
            if (startOfDayEventText != null)
                startOfDayEventText.text = $"TODAY:\n{GetEventDescription(currentEvent)}";
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            CloseStartOfDayWindow();
        }
    }

    public void CloseStartOfDayWindow()
    {
        if (startOfDayWindow != null) startOfDayWindow.SetActive(false);
        
        isDayActive = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private DailyEvent RollForEvent()
    {
        // Don't have an event for Day 1
        if (currentDay == 1) return DailyEvent.None;

        DailyEvent randomEvent = DailyEvent.None;

        int random100 = Random.Range(1, 101); // 1 to 100

        if (random100 <= 1 && illnessCount < 1) 
        {
             randomEvent = DailyEvent.Illness;
             illnessCount++;
        }
        else if (random100 <= 2 && infestationCount < 1)
        {
             randomEvent = DailyEvent.Infestation;
             infestationCount++;
        }
        else if (random100 <= 3 && bagyoCount < 2)
        {
             randomEvent = DailyEvent.Bagyo;
             bagyoCount++;
        }
        else if (random100 <= 5)
        {
             randomEvent = DailyEvent.Oversleep;
        }
        else if (random100 <= 7)
        {
             randomEvent = DailyEvent.Holiday;
        }
        else if (random100 <= 12)
        {
             randomEvent = DailyEvent.SchoolEvent;
        }
        else if (random100 <= 14 && vloggerCount < 2) 
        {
             randomEvent = DailyEvent.Vlogger;
             vloggerCount++;
        }
        
        return randomEvent;
    }

    private void ApplyEventEffects()
    {
        if (currentEvent == DailyEvent.Infestation)
        {
            Dispenser[] dispensers = FindObjectsOfType<Dispenser>();
            foreach(var d in dispensers) { d.currentAmount /= 2; d.Restock(0); }
            IngredientRack[] racks = FindObjectsOfType<IngredientRack>();
            foreach(var r in racks) { r.currentAmount /= 2; r.Restock(0); }
            Debug.Log("[EVENT] Infestation! Lost half ingredients");
        }
        
        if (currentEvent == DailyEvent.Oversleep)
        {
            gameTimeTimer = realSecondsPerGameHour * 1.5f; // Jump 1.5 hours ahead
            Debug.Log("[EVENT] Overslept! Preparation time shortened.");
        }
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

        if (eventHUDText != null)
        {
            string eventStr = "";
            if (currentEvent != DailyEvent.None)
                eventStr = $"Event: {currentEvent}";
            
            if (viralDaysRemaining > 0)
                eventStr += "\n[VIRAL EFFECT]";
            else if (viralFailedDaysRemaining > 0)
                eventStr += "\n[FLOPPED EFFECT]";
                
            eventHUDText.text = eventStr;
        }
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

    public void EndDay()
    {
        isDayActive = false;
        dayEndWindow.SetActive(true);

        // Unlock cursor for UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (currentEvent != DailyEvent.Illness && currentEvent != DailyEvent.Bagyo)
        {
            totalMoney -= dailyCost; // Normal daily cost
        }

        if (viralDaysRemaining > 0) viralDaysRemaining--;
        if (viralFailedDaysRemaining > 0) viralFailedDaysRemaining--;

        if (statsText != null)
        {
            statsText.text = $"DAY {currentDay} COMPLETE\n\n" +
                             $"Money Earned: ${moneyEarnedToday}\n" +
                             $"Daily Costs: ${dailyCost}\n" +
                             $"Total Balance: ${totalMoney}";
        }

        nextDayEvent = RollForEvent();

        if (endOfDayNewsText != null)
        {
            endOfDayNewsText.text = $"NEWS FORECAST:\n{GetEventForecast(nextDayEvent)}";
        }

        SaveGame();
        UpdateShopAmountsUI();
        UpdateUpgradesUI();
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.totalMoney = totalMoney;
        data.currentDay = currentDay;
        data.doughMakingUpgradeLevel = doughMakingUpgradeLevel;
        data.bakingUpgradeLevel = bakingUpgradeLevel;
        data.burnTimeUpgradeLevel = burnTimeUpgradeLevel;

        SaveSystem.Save(SaveSystem.SelectedSlot, data);
    }

    public void LoadGame()
    {
        SaveData data = SaveSystem.Load(SaveSystem.SelectedSlot);
        totalMoney = data.totalMoney;
        currentDay = data.currentDay;
        doughMakingUpgradeLevel = data.doughMakingUpgradeLevel;
        bakingUpgradeLevel = data.bakingUpgradeLevel;
        burnTimeUpgradeLevel = data.burnTimeUpgradeLevel;

        UpdateHUD();
        UpdateUpgradesUI();
        Debug.Log($"[LOAD] Slot {SaveSystem.SelectedSlot} Loaded. Day: {currentDay}");
    }

    private string GetEventDescription(DailyEvent e)
    {
        switch (e)
        {
            case DailyEvent.Oversleep: return "You overslept! Preparation time is shortened.";
            case DailyEvent.Bagyo: return "Typhoon! Customers will be extremely rare today.";
            case DailyEvent.Infestation: return "Pest Infestation! Half of your ingredients were ruined.";
            case DailyEvent.Vlogger: return "A famous vlogger might visit today. Serve them well!";
            case DailyEvent.Holiday: return "It's a Holiday! Fewer customers, but they buy in bulk.";
            case DailyEvent.SchoolEvent: return "School Event nearby! Many students, but small orders.";
            case DailyEvent.Illness: return "You got sick! Movement speed is reduced by 25%. Paid $100 for medicine.";
            default: return "Just a regular day.";
        }
    }

    private string GetEventForecast(DailyEvent e)
    {
        switch (e)
        {
            case DailyEvent.Oversleep: return "Citizens urged to set their alarms as strange fatigue sweeps the city.";
            case DailyEvent.Bagyo: return "Heavy rains and typhoon expected tomorrow. Stay safe!";
            case DailyEvent.Infestation: return "Health inspectors issue pest warning in the local area.";
            case DailyEvent.Vlogger: return "Rumors say a famous food vlogger is dropping by town!";
            case DailyEvent.Holiday: return "A local holiday is coming up tomorrow!";
            case DailyEvent.SchoolEvent: return "Local schools are preparing for a massive event.";
            case DailyEvent.Illness: return "Flu season is here. Drink water and rest up!";
            default: return "Normal weather and a regular day expected tomorrow.";
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

    public void BuyDoughUpgrade()
    {
        if (doughMakingUpgradeLevel < doughUpgradeCosts.Length)
        {
            int cost = doughUpgradeCosts[doughMakingUpgradeLevel];
            if (totalMoney >= cost)
            {
                totalMoney -= cost;
                doughMakingUpgradeLevel++;
                UpdateHUD();
                UpdateShopAmountsUI();
                UpdateUpgradesUI();
                SaveGame();
                Debug.Log($"[SHOP] Dough Making Upgraded to level {doughMakingUpgradeLevel}");
            }
        }
    }

    public void BuyBakingUpgrade()
    {
        if (bakingUpgradeLevel < bakingUpgradeCosts.Length)
        {
            int cost = bakingUpgradeCosts[bakingUpgradeLevel];
            if (totalMoney >= cost)
            {
                totalMoney -= cost;
                bakingUpgradeLevel++;
                UpdateHUD();
                UpdateShopAmountsUI();
                UpdateUpgradesUI();
                SaveGame();
                Debug.Log($"[SHOP] Baking Upgraded to level {bakingUpgradeLevel}");
            }
        }
    }

    public void BuyBurnTimeUpgrade()
    {
        if (burnTimeUpgradeLevel < burnUpgradeCosts.Length)
        {
            int cost = burnUpgradeCosts[burnTimeUpgradeLevel];
            if (totalMoney >= cost)
            {
                totalMoney -= cost;
                burnTimeUpgradeLevel++;
                UpdateHUD();
                UpdateShopAmountsUI();
                UpdateUpgradesUI();
                SaveGame();
                Debug.Log($"[SHOP] Burn Time Upgraded to level {burnTimeUpgradeLevel}");
            }
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

    public void UpdateUpgradesUI()
    {
        if (doughUpgradeText != null)
        {
            if (doughMakingUpgradeLevel < doughUpgradeCosts.Length)
                doughUpgradeText.text = $"Faster Dough Making\nLvl {doughMakingUpgradeLevel} -> {doughMakingUpgradeLevel + 1}\nCost: ${doughUpgradeCosts[doughMakingUpgradeLevel]}";
            else
                doughUpgradeText.text = "Faster Dough Making\nMAX LEVEL";
        }

        if (bakingUpgradeText != null)
        {
            if (bakingUpgradeLevel < bakingUpgradeCosts.Length)
                bakingUpgradeText.text = $"Faster Baking\nLvl {bakingUpgradeLevel} -> {bakingUpgradeLevel + 1}\nCost: ${bakingUpgradeCosts[bakingUpgradeLevel]}";
            else
                bakingUpgradeText.text = "Faster Baking\nMAX LEVEL";
        }

        if (burnTimeUpgradeText != null)
        {
            if (burnTimeUpgradeLevel < burnUpgradeCosts.Length)
                burnTimeUpgradeText.text = $"Longer Burn Time\nLvl {burnTimeUpgradeLevel} -> {burnTimeUpgradeLevel + 1}\nCost: ${burnUpgradeCosts[burnTimeUpgradeLevel]}";
            else
                burnTimeUpgradeText.text = "Longer Burn Time\nMAX LEVEL";
        }
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
