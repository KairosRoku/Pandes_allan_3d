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
    public bool allowDebugKeys = false;

    [Header("Time Settings")]
    public float realSecondsPerGameHour = 60f;
    public int startHour = 3;
    public int endHour = 12;
    public int serviceStartHour = 5;

    private float gameTimeTimer = 0f;
    public bool isDayActive = false;

    [Header("Economy")]
    public int totalMoney = 100;
    public int moneyEarnedToday = 0;
    public int currentDay = 1;
    public int gemsConvertedToday = 0;

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
    public GameObject watchAdButtonObj;

    [Header("HUD")]
    public GameObject hudPanel;
    public GameObject prepPhaseIndicator;
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI gemText;

    [Header("VFX")]
    public MoneyVFX moneyVFX;

    [Header("Shop Settings")]
    public int flourRestockCost = 10;
    public int sugarRestockCost = 10;
    public int waterRestockCost = 5;
    public int restockAmountPerPurchase = 5;

    [Header("Shop UI Texts")]
    public TextMeshProUGUI shopFlourAmountText;
    public TextMeshProUGUI shopSugarAmountText;
    public TextMeshProUGUI shopWaterAmountText;
    public TextMeshProUGUI shopMoneyText;

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
        if (currentDay == 1 && gameTimeTimer == 0)
        {
            LoadGame();
        }

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
            totalMoney -= 100;
        }

        if (startOfDayWindow != null && currentEvent != DailyEvent.None)
        {
            isDayActive = false;
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
        if (currentDay == 1) return DailyEvent.None;

        int random100 = Random.Range(1, 101);

        if (random100 <= 1 && illnessCount < 1) 
        {
             illnessCount++;
             return DailyEvent.Illness;
        }
        if (random100 <= 2 && infestationCount < 1)
        {
             infestationCount++;
             return DailyEvent.Infestation;
        }
        if (random100 <= 3 && bagyoCount < 2)
        {
             bagyoCount++;
             return DailyEvent.Bagyo;
        }
        if (random100 <= 5) return DailyEvent.Oversleep;
        if (random100 <= 7) return DailyEvent.Holiday;
        if (random100 <= 12) return DailyEvent.SchoolEvent;
        if (random100 <= 14 && vloggerCount < 2) 
        {
             vloggerCount++;
             return DailyEvent.Vlogger;
        }
        
        return DailyEvent.None;
    }

    private void ApplyEventEffects()
    {
        if (currentEvent == DailyEvent.Infestation)
        {
            foreach(var d in FindObjectsOfType<Dispenser>()) { d.currentAmount /= 2; d.Restock(0); }
            foreach(var r in FindObjectsOfType<IngredientRack>()) { r.currentAmount /= 2; r.Restock(0); }
        }
        
        if (currentEvent == DailyEvent.Oversleep)
        {
            gameTimeTimer = realSecondsPerGameHour * 1.5f;
        }
    }

    private void Update()
    {
        if (isDayActive)
        {
            gameTimeTimer += Time.deltaTime;
            
            if (allowDebugKeys && UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.f11Key.wasPressedThisFrame)
                {
                    float skipTime = (serviceStartHour - startHour) * realSecondsPerGameHour;
                    if (gameTimeTimer < skipTime) gameTimeTimer = skipTime;
                }
                if (UnityEngine.InputSystem.Keyboard.current.f12Key.wasPressedThisFrame)
                {
                    if (AdManager.Instance != null) AdManager.Instance.ApplyInstantDoughBuff();
                }
            }

            UpdateHUD();

            if (startHour + (gameTimeTimer / realSecondsPerGameHour) >= endHour)
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

        if (clockText != null) clockText.text = $"{hours:D2}:{minutes:D2} AM";
        if (moneyText != null) moneyText.text = $"${totalMoney}";
        if (dayText != null) dayText.text = $"Day {currentDay}";
        if (gemText != null) gemText.text = $"gem {(GemManager.Instance != null ? GemManager.Instance.totalGems : 0)}";

        if (prepPhaseIndicator != null)
            prepPhaseIndicator.SetActive(!IsServiceTime());

        if (eventHUDText != null)
        {
            string eventStr = "";
            if (currentEvent != DailyEvent.None) eventStr = $"Event: {currentEvent}";
            if (viralDaysRemaining > 0) eventStr += "\n[VIRAL EFFECT]";
            else if (viralFailedDaysRemaining > 0) eventStr += "\n[FLOPPED EFFECT]";

            if (AdManager.Instance != null)
            {
                string buffLabel = AdManager.Instance.GetActiveBuffLabel();
                if (!string.IsNullOrEmpty(buffLabel)) eventStr += $"\n{buffLabel}";
            }
            eventHUDText.text = eventStr;
        }
    }

    public bool IsServiceTime() => GetCurrentHour() >= serviceStartHour;

    public float GetCurrentHour() => startHour + (gameTimeTimer / realSecondsPerGameHour);

    public void AddMoney(int amount)
    {
        totalMoney += amount;
        moneyEarnedToday += amount;
        UpdateHUD();

        if (moneyVFX != null) moneyVFX.PlayBurst();

        if (moneyText != null)
        {
            var rt = moneyText.GetComponent<RectTransform>();
            if (rt != null)
            {
                StopCoroutine("WiggleMoneyRoutine");
                StartCoroutine(WiggleMoneyRoutine(rt));
            }
        }
    }

    private IEnumerator WiggleMoneyRoutine(RectTransform rt) => FlavorEffects.Wiggle(rt);

    public void EndDay()
    {
        isDayActive = false;
        dayEndWindow.SetActive(true);
        if (SFXManager.Instance != null) SFXManager.Instance.PlayDayEnd();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (currentEvent != DailyEvent.Illness && currentEvent != DailyEvent.Bagyo)
            totalMoney -= dailyCost;

        if (viralDaysRemaining > 0) viralDaysRemaining--;
        if (viralFailedDaysRemaining > 0) viralFailedDaysRemaining--;

        gemsConvertedToday = 0;
        if (GemManager.Instance != null && GemManager.Instance.totalGems > 0)
        {
            gemsConvertedToday = GemManager.Instance.totalGems;
            totalMoney += GemManager.Instance.ExchangeGemsForMoney();
        }

        if (statsText != null)
        {
            string gemLine = gemsConvertedToday > 0 ? $"Gems Exchanged: gem {gemsConvertedToday} → +${gemsConvertedToday * 10}\n" : "";
            statsText.text = $"DAY {currentDay} COMPLETE\n\nMoney Earned: ${moneyEarnedToday}\nDaily Costs: ${dailyCost}\n{gemLine}Total Balance: ${totalMoney}";
            StartCoroutine(FlavorEffects.WaveText(statsText, duration: 2.0f, amplitude: 8f, frequency: 2.5f));
        }

        nextDayEvent = RollForEvent();
        if (endOfDayNewsText != null) endOfDayNewsText.text = $"NEWS FORECAST:\n{GetEventForecast(nextDayEvent)}";
        if (watchAdButtonObj != null) watchAdButtonObj.SetActive(AdManager.Instance != null);

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

        if (WorldStateSaver.Instance != null) WorldStateSaver.Instance.CaptureWorldState(data);
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

        if (WorldStateSaver.Instance != null) WorldStateSaver.Instance.RestoreWorldState(data);
        if (GemManager.Instance != null)
        {
            GemManager.Instance.totalGems = data.totalGems;
            GemManager.Instance.UpdateHUD();
        }

        UpdateHUD();
        UpdateUpgradesUI();
    }

    public void OpenGemShop() { if (GemManager.Instance != null) GemManager.Instance.OpenGemShop(); }
    public void CloseGemShop() { if (GemManager.Instance != null) GemManager.Instance.CloseGemShop(); }
    public void ShowAdOffer() { if (AdManager.Instance != null) AdManager.Instance.ShowAdOffer(); }

    private string GetEventDescription(DailyEvent e)
    {
        return e switch
        {
            DailyEvent.Oversleep => "You overslept! Preparation time is shortened.",
            DailyEvent.Bagyo => "Typhoon! Customers will be extremely rare today.",
            DailyEvent.Infestation => "Pest Infestation! Half of your ingredients were ruined.",
            DailyEvent.Vlogger => "A famous vlogger might visit today. Serve them well!",
            DailyEvent.Holiday => "It's a Holiday! Fewer customers, but they buy in bulk.",
            DailyEvent.SchoolEvent => "School Event nearby! Many students, but small orders.",
            DailyEvent.Illness => "You got sick! Movement speed is reduced by 25%. Paid $100 for medicine.",
            _ => "Just a regular day."
        };
    }

    private string GetEventForecast(DailyEvent e)
    {
        return e switch
        {
            DailyEvent.Oversleep => "Citizens urged to set their alarms as strange fatigue sweeps the city.",
            DailyEvent.Bagyo => "Heavy rains and typhoon expected tomorrow. Stay safe!",
            DailyEvent.Infestation => "Health inspectors issue pest warning in the local area.",
            DailyEvent.Vlogger => "Rumors say a famous food vlogger is dropping by town!",
            DailyEvent.Holiday => "A local holiday is coming up tomorrow!",
            DailyEvent.SchoolEvent => "Local schools are preparing for a massive event.",
            DailyEvent.Illness => "Flu season is here. Drink water and rest up!",
            _ => "Normal weather and a regular day expected tomorrow."
        };
    }

    public void NextDay() { currentDay++; StartDay(); }

    public void BuyItem(ItemType type, int cost)
    {
        if (totalMoney >= cost)
        {
            totalMoney -= cost;
            UpdateHUD();
            if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
        }
    }

    public void BuyFlour() => TryRestock(ItemType.Flour, flourRestockCost);
    public void BuySugar() => TryRestock(ItemType.Sugar, sugarRestockCost);
    public void BuyWater() => TryRestock(ItemType.Water, waterRestockCost);

    private void TryRestock(ItemType type, int cost)
    {
        if (totalMoney >= cost)
        {
            totalMoney -= cost;
            UpdateHUD();
            if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
            foreach (var d in FindObjectsOfType<Dispenser>()) if (d.itemType == type) d.Restock(restockAmountPerPurchase);
            foreach (var r in FindObjectsOfType<IngredientRack>()) if (r.itemType == type) r.Restock(restockAmountPerPurchase);
            UpdateShopAmountsUI();
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
                UpdateHUD(); UpdateShopAmountsUI(); UpdateUpgradesUI(); SaveGame();
                if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
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
                UpdateHUD(); UpdateShopAmountsUI(); UpdateUpgradesUI(); SaveGame();
                if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
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
                UpdateHUD(); UpdateShopAmountsUI(); UpdateUpgradesUI(); SaveGame();
                if (SFXManager.Instance != null) SFXManager.Instance.PlayBuy();
            }
        }
    }

    public void UpdateShopAmountsUI()
    {
        if (shopMoneyText != null) shopMoneyText.text = $"Balance: ${totalMoney}";
        if (shopFlourAmountText != null) shopFlourAmountText.text = $"Owned: {GetTotalIngredientAmount(ItemType.Flour)}";
        if (shopSugarAmountText != null) shopSugarAmountText.text = $"Owned: {GetTotalIngredientAmount(ItemType.Sugar)}";
        if (shopWaterAmountText != null) shopWaterAmountText.text = $"Owned: {GetTotalIngredientAmount(ItemType.Water)}";
    }

    public void UpdateUpgradesUI()
    {
        if (doughUpgradeText != null) doughUpgradeText.text = (doughMakingUpgradeLevel < doughUpgradeCosts.Length) ? $"Faster Dough Making\nLvl {doughMakingUpgradeLevel} -> {doughMakingUpgradeLevel + 1}\nCost: ${doughUpgradeCosts[doughMakingUpgradeLevel]}" : "Faster Dough Making\nMAX LEVEL";
        if (bakingUpgradeText != null) bakingUpgradeText.text = (bakingUpgradeLevel < bakingUpgradeCosts.Length) ? $"Faster Baking\nLvl {bakingUpgradeLevel} -> {bakingUpgradeLevel + 1}\nCost: ${bakingUpgradeCosts[bakingUpgradeLevel]}" : "Faster Baking\nMAX LEVEL";
        if (burnTimeUpgradeText != null) burnTimeUpgradeText.text = (burnTimeUpgradeLevel < burnUpgradeCosts.Length) ? $"Longer Burn Time\nLvl {burnTimeUpgradeLevel} -> {burnTimeUpgradeLevel + 1}\nCost: ${burnUpgradeCosts[burnTimeUpgradeLevel]}" : "Longer Burn Time\nMAX LEVEL";
    }

    private int GetTotalIngredientAmount(ItemType type)
    {
        int total = 0;
        foreach (var d in FindObjectsOfType<Dispenser>()) if (d.itemType == type) total += d.currentAmount;
        foreach (var r in FindObjectsOfType<IngredientRack>()) if (r.itemType == type) total += r.currentAmount;
        return total;
    }
}
