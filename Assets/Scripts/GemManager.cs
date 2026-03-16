using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the premium "Gems" currency.
/// - 3 purchasable packages (mock): ₱50→100💎, ₱100→250💎, ₱500→2000💎
/// - Gems convert to money at end-of-day at 1:10 ratio (auto).
/// - Built-in manual converter: player can exchange gems any time.
/// - Gems persist via SaveData.
/// </summary>
public class GemManager : MonoBehaviour
{
    public static GemManager Instance;

    [Header("Gem Balance")]
    public int totalGems = 0;

    // ─── Packages: 3 tiers ──────────────────────────────────────────
    public static readonly (int gems, int price, string label)[] GemPackages = new (int, int, string)[]
    {
        (100,   50,  "100 💎\n₱50"),
        (250,  100,  "250 💎\n₱100"),
        (2000, 500,  "2000 💎\n₱500"),
    };

    [Header("HUD")]
    public TextMeshProUGUI gemHUDText;

    // ─── Gem Shop Panel ─────────────────────────────────────────────
    [Header("Gem Shop Panel")]
    public GameObject gemShopPanel;

    // Balance shown inside the shop
    public TextMeshProUGUI gemBalanceText;

    // 3 button label texts (assign in Inspector)
    public TextMeshProUGUI[] packageButtonTexts; // length = 3

    // ─── Built-in Gem Converter (inside the shop panel) ─────────────
    [Header("Gem Converter (inside Shop Panel)")]
    [Tooltip("TMP text that shows how much money a conversion would yield.")]
    public TextMeshProUGUI converterPreviewText;  // e.g. "💎 50 → ₱500"
    [Tooltip("Input field where the player types the number of gems to convert.")]
    public TMP_InputField converterInputField;
    [Tooltip("Button that executes the conversion.")]
    public Button converterButton;

    // ─── Lifecycle ───────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    // ─── Public API ──────────────────────────────────────────────────

    public void AddGems(int amount)
    {
        totalGems += amount;
        UpdateHUD();
        RefreshShopUI();
        Debug.Log($"[GEMS] +{amount} gems. Total: {totalGems}");
    }

    /// <summary>
    /// End-of-day auto exchange: all gems → money at 1 gem = ₱10.
    /// Returns the peso amount added.
    /// </summary>
    public int ExchangeGemsForMoney()
    {
        int moneyGained = totalGems * 10;
        Debug.Log($"[GEMS] Auto-exchange: {totalGems} gems → ₱{moneyGained}");
        totalGems = 0;
        UpdateHUD();
        return moneyGained;
    }

    public void UpdateHUD()
    {
        if (gemHUDText != null)
            gemHUDText.text = $"💎 {totalGems}";
    }

    // ─── Gem Shop ────────────────────────────────────────────────────

    public void OpenGemShop()
    {
        if (gemShopPanel == null) return;
        gemShopPanel.SetActive(true);
        RefreshShopUI();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseGemShop()
    {
        if (gemShopPanel == null) return;
        gemShopPanel.SetActive(false);

        // Restore cursor state
        bool dayActive = GameManager.Instance != null && GameManager.Instance.isDayActive;
        if (dayActive && !PauseMenuUI.isPaused)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void RefreshShopUI()
    {
        if (gemBalanceText != null)
            gemBalanceText.text = $"Your Gems: 💎 {totalGems}";

        for (int i = 0; i < packageButtonTexts.Length && i < GemPackages.Length; i++)
        {
            if (packageButtonTexts[i] != null)
                packageButtonTexts[i].text = GemPackages[i].label;
        }

        // Reset converter preview
        RefreshConverterPreview();
    }

    // ─── Package purchase ────────────────────────────────────────────

    /// <summary>Called by each gem shop button (index 0-2).</summary>
    public void BuyGemPackage(int index)
    {
        if (index < 0 || index >= GemPackages.Length) return;
        int gemAmount = GemPackages[index].gems;
        int price     = GemPackages[index].price;
        AddGems(gemAmount);
        Debug.Log($"[GEM SHOP] Package {index}: +{gemAmount} 💎 (mock ₱{price} purchase).");
    }

    // Convenience helpers for Unity Button.OnClick (no int param)
    public void BuyPackage0() => BuyGemPackage(0);
    public void BuyPackage1() => BuyGemPackage(1);
    public void BuyPackage2() => BuyGemPackage(2);

    // ─── Manual Gem Converter ────────────────────────────────────────

    /// <summary>
    /// Called by the TMP_InputField's OnValueChanged event.
    /// Updates the preview text to show how much money the entered gems convert to.
    /// </summary>
    public void OnConverterInputChanged(string input)
    {
        RefreshConverterPreview();
    }

    private void RefreshConverterPreview()
    {
        if (converterPreviewText == null) return;

        int amount = ParseConverterInput();
        if (amount <= 0)
        {
            converterPreviewText.text = "Enter gems to convert (1 💎 = ₱10)";
        }
        else if (amount > totalGems)
        {
            converterPreviewText.text = $"❌ Not enough gems! You have 💎 {totalGems}";
        }
        else
        {
            int pesos = amount * 10;
            converterPreviewText.text = $"💎 {amount} → ₱{pesos}";
        }

        // Enable/disable the convert button
        if (converterButton != null)
            converterButton.interactable = (amount > 0 && amount <= totalGems);
    }

    /// <summary>
    /// Called by the "Convert" button's OnClick.
    /// Converts the entered gem amount to money immediately.
    /// </summary>
    public void OnConvertButtonClicked()
    {
        int amount = ParseConverterInput();
        if (amount <= 0 || amount > totalGems) return;

        int pesos = amount * 10;
        totalGems -= amount;

        if (GameManager.Instance != null)
            GameManager.Instance.AddMoney(pesos);

        Debug.Log($"[GEM CONVERTER] Converted 💎{amount} → ₱{pesos}. Gems left: {totalGems}");

        // Reset input and refresh UI
        if (converterInputField != null)
            converterInputField.text = "";

        UpdateHUD();
        RefreshShopUI();
    }

    private int ParseConverterInput()
    {
        if (converterInputField == null) return 0;
        if (int.TryParse(converterInputField.text, out int val) && val > 0)
            return val;
        return 0;
    }
}
