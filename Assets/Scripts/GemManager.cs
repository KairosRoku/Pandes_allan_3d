using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GemManager : MonoBehaviour
{
    public static GemManager Instance;

    [Header("Gem Balance")]
    public int totalGems = 0;

    public static readonly (int gems, int price, string label)[] GemPackages = new (int, int, string)[]
    {
        (100,   50,  "100 gem\n₱50"),
        (250,  100,  "250 gem\n₱100"),
        (2000, 500,  "2000 gem\n₱500"),
    };

    [Header("HUD")]
    public TextMeshProUGUI gemHUDText;

    [Header("Gem Shop Panel")]
    public GameObject gemShopPanel;
    public TextMeshProUGUI gemBalanceText;
    public TextMeshProUGUI[] packageButtonTexts;

    [Tooltip("TMP text that shows how much money a conversion would yield.")]
    public TextMeshProUGUI converterPreviewText;
    [Tooltip("Input field where the player types the number of gems to convert.")]
    public TMP_InputField converterInputField;
    [Tooltip("Button that executes the conversion.")]
    public Button converterButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    public void AddGems(int amount)
    {
        totalGems += amount;
        UpdateHUD();
        RefreshShopUI();
        Debug.Log($"[GEMS] +{amount} gems. Total: {totalGems}");
    }

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
            gemHUDText.text = $"gem {totalGems}";
    }

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
            gemBalanceText.text = $"Your Gems: gem {totalGems}";

        for (int i = 0; i < packageButtonTexts.Length && i < GemPackages.Length; i++)
        {
            if (packageButtonTexts[i] != null)
                packageButtonTexts[i].text = GemPackages[i].label;
        }

        RefreshConverterPreview();
    }

    public void BuyGemPackage(int index)
    {
        if (index < 0 || index >= GemPackages.Length) return;
        int gemAmount = GemPackages[index].gems;
        int price     = GemPackages[index].price;
        AddGems(gemAmount);
        Debug.Log($"[GEM SHOP] Package {index}: +{gemAmount} gem (mock ₱{price} purchase).");
    }

    public void BuyPackage0() => BuyGemPackage(0);
    public void BuyPackage1() => BuyGemPackage(1);
    public void BuyPackage2() => BuyGemPackage(2);

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
            converterPreviewText.text = "Enter gems to convert (1 gem = ₱10)";
        }
        else if (amount > totalGems)
        {
            converterPreviewText.text = $"Not enough gems! You have gem {totalGems}";
        }
        else
        {
            int pesos = amount * 10;
            converterPreviewText.text = $"gem {amount} → ₱{pesos}";
        }

        if (converterButton != null)
            converterButton.interactable = (amount > 0 && amount <= totalGems);
    }

    public void OnConvertButtonClicked()
    {
        int amount = ParseConverterInput();
        if (amount <= 0 || amount > totalGems) return;

        int pesos = amount * 10;
        totalGems -= amount;

        if (GameManager.Instance != null)
            GameManager.Instance.AddMoney(pesos);

        Debug.Log($"[GEM CONVERTER] Converted gem {amount} → ₱{pesos}. Gems left: {totalGems}");

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
