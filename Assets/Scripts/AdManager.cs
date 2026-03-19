using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum DailyBuff
{
    None,
    SpeedBoost,      // Player is 25% faster for the day
    InstantDough,    // Uses player's materials to make as much dough as possible instantly
    NoMinigame       // Skip kneading & shaping minigames for the day
}

/// <summary>
/// Manages mock advertisements shown at the end of the day.
/// Watching a 10-second ad rewards a random DailyBuff for the next day.
/// </summary>
public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    // Active buff for the current game day
    [HideInInspector] public DailyBuff activeBuffToday = DailyBuff.None;
    
    // Buff acquired from watching an ad, applied on the next day
    private DailyBuff nextDayBuff = DailyBuff.None;

    [Header("Ad UI Panel")]
    public GameObject adPanel;
    public TextMeshProUGUI adCountdownText;
    public TextMeshProUGUI adFlavorText;
    public Button watchAdButton;
    public Button skipAdButton;   // becomes active only after ad finishes

    [Header("Buff Result UI")]
    public GameObject buffResultPanel;
    public TextMeshProUGUI buffResultText;

    [Header("Ad Settings")]
    public float adDuration = 10f;

    private bool isWatchingAd = false;
    private bool hasWatchedAdToday = false;

    // Flavor texts that cycle during the fake ad
    private static readonly string[] AdFlavors = new string[]
    {
        "🍞 PANDESAL PRO — Bake faster, bake better!",
        "💰 INVEST NOW — Triple your dough today!",
        "🧹 CLEAN SWEEP — Pest-free guaranteed!",
        "🚀 SUGAR RUSH ENERGY — Power through your shift!",
        "📱 DOWNLOAD BREAD RUSH — The #1 bakery game!",
        "🎉 LIMITED OFFER — Free flour with every bag!",
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (adPanel != null) adPanel.SetActive(false);
        if (buffResultPanel != null) buffResultPanel.SetActive(false);
    }

    // ─── Called from End-Day UI ─────────────────────────────────────

    /// <summary>Opens the ad panel. Wired directly to the "Watch Ad" button in DayEndWindow.</summary>
    public void ShowAdOffer()
    {
        if (adPanel == null)
        {
            Debug.LogWarning("[AdManager] adPanel is not assigned in the Inspector!");
            return;
        }

        // Stop any previous ad coroutine so reopening always starts clean
        StopAllCoroutines();
        isWatchingAd = false;

        // Reset all UI state
        if (hasWatchedAdToday)
        {
            if (adCountdownText != null) adCountdownText.text = "You have already received a buff today.\nCome back tomorrow!";
            if (adFlavorText != null)    adFlavorText.text = "";
            if (watchAdButton != null)   watchAdButton.interactable = false;
        }
        else
        {
            if (adCountdownText != null) adCountdownText.text = "Watch a 10-second ad\nfor a FREE daily buff!";
            if (adFlavorText != null)    adFlavorText.text = "";
            if (watchAdButton != null)   watchAdButton.interactable = true;
        }
        
        if (skipAdButton != null)    skipAdButton.gameObject.SetActive(false);
        if (buffResultPanel != null) buffResultPanel.SetActive(false);

        adPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseAdPanel()
    {
        StopAllCoroutines();
        if (adPanel != null) adPanel.SetActive(false);
        if (buffResultPanel != null) buffResultPanel.SetActive(false);
        isWatchingAd = false;
    }

    // Called by "Watch Ad" button
    public void OnWatchAdClicked()
    {
        if (isWatchingAd) return;
        if (watchAdButton != null) watchAdButton.interactable = false;
        StartCoroutine(PlayAd());
    }

    private IEnumerator PlayAd()
    {
        isWatchingAd = true;
        float elapsed = 0f;
        int flavorIndex = 0;

        while (elapsed < adDuration)
        {
            float remaining = adDuration - elapsed;
            if (adCountdownText != null)
                adCountdownText.text = $"Ad playing… {remaining:F0}s";

            // Cycle ad flavor text every ~2 seconds
            int newFlavor = Mathf.FloorToInt(elapsed / 1.5f) % AdFlavors.Length;
            if (newFlavor != flavorIndex)
            {
                flavorIndex = newFlavor;
                if (adFlavorText != null)
                    adFlavorText.text = AdFlavors[flavorIndex];
            }

            elapsed += Time.unscaledDeltaTime; // use unscaled so it works even if timescale is 0
            yield return null;
        }

        // Ad finished!
        isWatchingAd = false;
        hasWatchedAdToday = true;
        GrantRandomBuff();

        if (adCountdownText != null) adCountdownText.text = "Ad Complete! Buff granted!";
        if (skipAdButton != null) skipAdButton.gameObject.SetActive(true);
    }

    private void GrantRandomBuff()
    {
        // Pick a random buff (1–3)
        int roll = Random.Range(1, 4);
        DailyBuff buff = (DailyBuff)roll;
        nextDayBuff = buff;

        // Show buff result
        if (buffResultPanel != null)
        {
            buffResultPanel.SetActive(true);
            if (buffResultText != null)
                buffResultText.text = $"🎁 BUFF GRANTED:\n{GetBuffDescription(buff)}";
        }

        // If Instant Dough buff — apply immediately (uses available ingredients)
        if (buff == DailyBuff.InstantDough)
        {
            ApplyInstantDoughBuff();
        }

        Debug.Log($"[AD] Buff granted: {buff} — {GetBuffDescription(buff)}");
    }

    public void ApplyInstantDoughBuff()
    {
        // Count available sets of ingredients (flour + sugar + water = 1 dough)
        int flour = GetIngredientTotal(ItemType.Flour);
        int sugar = GetIngredientTotal(ItemType.Sugar);
        int water = GetIngredientTotal(ItemType.Water);

        int doughSets = Mathf.Min(flour, sugar, water);
        if (doughSets <= 0)
        {
            Debug.Log("[AD] Instant Dough: No full ingredient sets available.");
            return;
        }

        // Consume ingredients and add dough to the bin
        ConsumeIngredients(doughSets);

        DoughBin[] bins = FindObjectsOfType<DoughBin>();
        foreach (var bin in bins)
        {
            for (int i = 0; i < doughSets; i++)
                bin.AddDough();
        }

        Debug.Log($"[AD] Instant Dough: Produced {doughSets} dough from available ingredients.");
    }

    private int GetIngredientTotal(ItemType type)
    {
        int total = 0;
        foreach (var d in FindObjectsOfType<Dispenser>())
            if (d.itemType == type) total += d.currentAmount;
        foreach (var r in FindObjectsOfType<IngredientRack>())
            if (r.itemType == type) total += r.currentAmount;
        return total;
    }

    private void ConsumeIngredients(int sets)
    {
        ConsumeFromSource(ItemType.Flour, sets);
        ConsumeFromSource(ItemType.Sugar, sets);
        ConsumeFromSource(ItemType.Water, sets);
    }

    private void ConsumeFromSource(ItemType type, int amount)
    {
        int remaining = amount;

        foreach (var d in FindObjectsOfType<Dispenser>())
        {
            if (d.itemType != type || remaining <= 0) continue;
            int take = Mathf.Min(d.currentAmount, remaining);
            d.currentAmount -= take;
            d.Restock(0); // refresh UI
            remaining -= take;
        }

        foreach (var r in FindObjectsOfType<IngredientRack>())
        {
            if (r.itemType != type || remaining <= 0) continue;
            int take = Mathf.Min(r.currentAmount, remaining);
            r.currentAmount -= take;
            r.Restock(0); // refresh UI
            remaining -= take;
        }
    }

    // ─── Buff Queries (read by other systems) ──────────────────────

    public bool HasSpeedBoost() => activeBuffToday == DailyBuff.SpeedBoost;
    public bool HasInstantDough() => activeBuffToday == DailyBuff.InstantDough;
    public bool HasNoMinigame() => activeBuffToday == DailyBuff.NoMinigame;

    /// <summary>Call this at the start of each new day to lock in the buff.</summary>
    public void StartNewDay()
    {
        activeBuffToday = nextDayBuff;
        nextDayBuff = DailyBuff.None;
        hasWatchedAdToday = false;
        Debug.Log($"[AD] New day started. Active Buff: {activeBuffToday}");
    }

    private string GetBuffDescription(DailyBuff buff)
    {
        return buff switch
        {
            DailyBuff.SpeedBoost   => "⚡ Speed Boost — 25% faster movement today!",
            DailyBuff.InstantDough => "🌾 Instant Dough — All your ingredients turned into dough!",
            DailyBuff.NoMinigame   => "✂️ No Minigame — Kneading & Shaping skipped automatically!",
            _                      => "No buff."
        };
    }

    public string GetActiveBuffLabel()
    {
        if (activeBuffToday == DailyBuff.None) return "";
        return $"BUFF: {GetBuffDescription(activeBuffToday)}";
    }
}
