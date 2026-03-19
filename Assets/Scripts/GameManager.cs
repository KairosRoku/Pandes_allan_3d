using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

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

    [Header("Debug")]
    [Tooltip("If checked, pressing F11 skips to Service Phase and F12 gives instant dough.")]
    public bool allowDebugKeys = false;

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
    public int gemsConvertedToday = 0; // for end-of-day stats

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
    // "Watch Ad" button object inside the dayEndWindow (optional)
    public GameObject watchAdButtonObj;

    [Header("HUD")]
    public GameObject hudPanel;
    public GameObject prepPhaseIndicator;
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI gemText; // Gem HUD display

    [Header("VFX")]
    public MoneyVFX moneyVFX; // Assign in Inspector — place near the CustomerWindow

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

        // Apply ad buff and clear lockout for the new day
        if (AdManager.Instance != null)
            AdManager.Instance.StartNewDay();

        currentEvent = nextDayEvent;

        gameTimeTimer = 0f;
        moneyEarnedToday = 0;
        gemsConvertedToday = 0;
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
            
            // Debug / Testing Skips
            if (allowDebugKeys && UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.f11Key.wasPressedThisFrame)
                {
                    float skipTime = (serviceStartHour - startHour) * realSecondsPerGameHour;
                    if (gameTimeTimer < skipTime)
                    {
                        gameTimeTimer = skipTime;
                        Debug.Log("[DEBUG] F11 Pressed: Skipped prep phase to 5 AM!");
                    }
                }
                if (UnityEngine.InputSystem.Keyboard.current.f12Key.wasPressedThisFrame)
                {
                    if (AdManager.Instance != null)
                    {
                        Debug.Log("[DEBUG] F12 Pressed: Applying Instant Dough!");
                        AdManager.Instance.ApplyInstantDoughBuff();
                    }
                }
            }

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

        // Gem HUD
        if (gemText != null)
            gemText.text = $"💎 {(GemManager.Instance != null ? GemManager.Instance.totalGems : 0)}";

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

            // Show active ad buff if any
            if (AdManager.Instance != null)
            {
                string buffLabel = AdManager.Instance.GetActiveBuffLabel();
                if (!string.IsNullOrEmpty(buffLabel))
                    eventStr += $"\n{buffLabel}";
            }
                
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

        // Trigger coin burst VFX at the selling point
        if (moneyVFX != null) moneyVFX.PlayBurst();

        // Wiggle the money text to provide juicy feedback
        if (moneyText != null)
        {
            var rt = moneyText.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Stop any existing wiggle before starting a new one
                StopCoroutine("WiggleMoneyRoutine");
                StartCoroutine(WiggleMoneyRoutine(rt));
            }
        }
    }

    private IEnumerator WiggleMoneyRoutine(RectTransform rt)
    {
        yield return FlavorEffects.Wiggle(rt);
    }

    public void EndDay()
    {
        isDayActive = false;
        dayEndWindow.SetActive(true);
        
        if (SFXManager.Instance != null) SFXManager.Instance.PlayDayEnd();

        // Unlock cursor for UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (currentEvent != DailyEvent.Illness && currentEvent != DailyEvent.Bagyo)
        {
            totalMoney -= dailyCost; // Normal daily cost
        }

        if (viralDaysRemaining > 0) viralDaysRemaining--;
        if (viralFailedDaysRemaining > 0) viralFailedDaysRemaining--;

        // Exchange gems → money (1:10)
        gemsConvertedToday = 0;
        if (GemManager.Instance != null && GemManager.Instance.totalGems > 0)
        {
            gemsConvertedToday = GemManager.Instance.totalGems;
            int gemMoney = GemManager.Instance.ExchangeGemsForMoney();
            totalMoney += gemMoney;
        }

        if (statsText != null)
        {
            string gemLine = gemsConvertedToday > 0
                ? $"Gems Exchanged: 💎{gemsConvertedToday} → +${gemsConvertedToday * 10}\n"
                : "";
            statsText.text = $"DAY {currentDay} COMPLETE\n\n" +
                             $"Money Earned: ${moneyEarnedToday}\n" +
                             $"Daily Costs: ${dailyCost}\n" +
                             gemLine +
                             $"Total Balance: ${totalMoney}";

            // Wave the money text for a satisfying end-of-day payoff
            StartCoroutine(FlavorEffects.WaveText(statsText, duration: 2.0f, amplitude: 8f, frequency: 2.5f));
        }

        nextDayEvent = RollForEvent();

        if (endOfDayNewsText != null)
        {
            endOfDayNewsText.text = $"NEWS FORECAST:\n{GetEventForecast(nextDayEvent)}";
        }

        // Show the Ad offer button if AdManager is present
        if (watchAdButtonObj != null)
            watchAdButtonObj.SetActive(AdManager.Instance != null);

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
        data.totalGems = GemManager.Instance != null ? GemManager.Instance.totalGems : 0;

        if (WorldStateSaver.Instance != null)
        {
            WorldStateSaver.Instance.CaptureWorldState(data);
        }

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

        if (WorldStateSaver.Instance != null)
        {
            WorldStateSaver.Instance.RestoreWorldState(data);
        }

        // Load gems
        if (GemManager.Instance != null)
        {
            GemManager.Instance.totalGems = data.totalGems;
            GemManager.Instance.UpdateHUD();
        }

        UpdateHUD();
        UpdateUpgradesUI();
        Debug.Log($"[LOAD] Slot {SaveSystem.SelectedSlot} Loaded. Day: {currentDay}");
    }

    // ─── Gem Shop helpers (can be called from UI buttons) ────────────

    public void OpenGemShop()
    {
        if (GemManager.Instance != null)
            GemManager.Instance.OpenGemShop();
    }

    public void CloseGemShop()
    {
        if (GemManager.Instance != null)
            GemManager.Instance.CloseGemShop();
    }

    // ─── Ad helpers ──────────────────────────────────────────────────

    public void ShowAdOffer()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.ShowAdOffer();
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
            if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
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
            if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
            
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
                if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
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
                if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
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
                if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
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
