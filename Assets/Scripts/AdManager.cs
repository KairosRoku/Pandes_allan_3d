using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public enum DailyBuff
{
    None,
    SpeedBoost,
    InstantDough,
    NoMinigame
}

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

    [HideInInspector] public DailyBuff activeBuffToday = DailyBuff.None;
    private DailyBuff nextDayBuff = DailyBuff.None;

    [Header("Ad UI Panel")]
    public GameObject adPanel;
    public TextMeshProUGUI adCountdownText;
    public TextMeshProUGUI adFlavorText;
    public Button watchAdButton;
    public Button skipAdButton;

    [Header("Ad Settings")]
    public float adDuration = 10f;

    private bool isWatchingAd = false;
    private bool hasWatchedAdToday = false;

    private static readonly string[] AdFlavors = new string[]
    {
        "PANDESAL PRO — Bake faster, bake better!",
        "INVEST NOW — Triple your dough today!",
        "CLEAN SWEEP — Pest-free guaranteed!",
        "SUGAR RUSH ENERGY — Power through your shift!",
        "DOWNLOAD BREAD RUSH — The #1 bakery game!",
        "LIMITED OFFER — Free flour with every bag!",
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (adPanel != null) adPanel.SetActive(false);
    }

    public void ShowAdOffer()
    {
        if (adPanel == null) return;

        StopAllCoroutines();
        isWatchingAd = false;

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
        adPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseAdPanel()
    {
        StopAllCoroutines();
        if (adPanel != null) adPanel.SetActive(false);
        isWatchingAd = false;
    }

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

            int newFlavor = Mathf.FloorToInt(elapsed / 1.5f) % AdFlavors.Length;
            if (newFlavor != flavorIndex)
            {
                flavorIndex = newFlavor;
                if (adFlavorText != null)
                    adFlavorText.text = AdFlavors[flavorIndex];
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        isWatchingAd = false;
        hasWatchedAdToday = true;
        string buffDesc = GrantRandomBuff();

        if (adCountdownText != null) adCountdownText.text = $"AD COMPLETE!\n{buffDesc}";
        if (skipAdButton != null) skipAdButton.gameObject.SetActive(true);
    }

    private string GrantRandomBuff()
    {
        int roll = Random.Range(1, 4);
        DailyBuff buff = (DailyBuff)roll;
        nextDayBuff = buff;

        if (buff == DailyBuff.InstantDough)
        {
            ApplyInstantDoughBuff();
        }

        string desc = GetBuffDescription(buff);
        Debug.Log($"[AD] Buff granted: {buff} — {desc}");
        return desc;
    }

    public void ApplyInstantDoughBuff()
    {
        int flour = GetIngredientTotal(ItemType.Flour);
        int sugar = GetIngredientTotal(ItemType.Sugar);
        int water = GetIngredientTotal(ItemType.Water);

        int doughSets = Mathf.Min(flour, sugar, water);
        if (doughSets <= 0) return;

        ConsumeIngredients(doughSets);

        foreach (var bin in FindObjectsOfType<DoughBin>())
        {
            for (int i = 0; i < doughSets; i++)
                bin.AddDough();
        }
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
            d.Restock(0);
            remaining -= take;
        }
        foreach (var r in FindObjectsOfType<IngredientRack>())
        {
            if (r.itemType != type || remaining <= 0) continue;
            int take = Mathf.Min(r.currentAmount, remaining);
            r.currentAmount -= take;
            r.Restock(0);
            remaining -= take;
        }
    }

    public bool HasSpeedBoost() => activeBuffToday == DailyBuff.SpeedBoost;
    public bool HasInstantDough() => activeBuffToday == DailyBuff.InstantDough;
    public bool HasNoMinigame() => activeBuffToday == DailyBuff.NoMinigame;

    public void StartNewDay()
    {
        activeBuffToday = nextDayBuff;
        nextDayBuff = DailyBuff.None;
        hasWatchedAdToday = false;
    }

    private string GetBuffDescription(DailyBuff buff)
    {
        return buff switch
        {
            DailyBuff.SpeedBoost   => "Speed Boost — 25% faster movement today!",
            DailyBuff.InstantDough => "Instant Dough — All your ingredients turned into dough!",
            DailyBuff.NoMinigame   => "No Minigame — Kneading & Shaping skipped automatically!",
            _                      => "No buff."
        };
    }

    public string GetActiveBuffLabel()
    {
        if (activeBuffToday == DailyBuff.None) return "";
        return $"BUFF: {GetBuffDescription(activeBuffToday)}";
    }
}
