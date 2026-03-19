using UnityEngine;
using TMPro;

public class IngredientRack : MonoBehaviour, IInteractable, ISaveable
{
    public GameObject ingredientPrefab;
    public ItemType itemType;

    [Header("Limitation Settings")]
    public bool isLimited = false;
    public int maxAmount = 10;
    public int currentAmount = 10;
    
    [Tooltip("Reference to the 3D Text or UI Text to show amount")]
    public TMP_Text amountText;

    private void Start()
    {
        if (isLimited)
        {
            UpdateAmountText();
        }
        else if (amountText != null)
        {
            amountText.text = "∞";
        }
    }

    public void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            GameObject held = player.GetHeldItem();
            var data = held.GetComponentInChildren<ItemData>();
            if (data != null && data.itemType == this.itemType)
            {
                Destroy(player.RemoveHeldItem());
                if (isLimited)
                {
                    currentAmount++;
                    if (currentAmount > maxAmount) currentAmount = maxAmount;
                    UpdateAmountText(wiggle: true);
                }
                Debug.Log($"[INGREDIENT RACK] Player returned {itemType}.");
            }
        }
        else if (ingredientPrefab != null)
        {
            if (isLimited && currentAmount <= 0)
            {
                Debug.Log($"[INGREDIENT RACK] {itemType} is empty!");
                return;
            }

            GameObject newIngredient = Instantiate(ingredientPrefab, player.holdPoint.position, player.holdPoint.rotation);
            player.PickUpItem(newIngredient);

            if (isLimited)
            {
                currentAmount--;
                UpdateAmountText(wiggle: true);
            }
        }
    }

    public string GetInteractText(PlayerController player)
    {
        if (isLimited)
        {
            if (currentAmount <= 0)
                return itemType.ToString() + " Empty!";
            return "Pick Up " + itemType.ToString() + $" ({currentAmount} left)";
        }
        return "Pick Up " + itemType.ToString();
    }

    public void Restock(int amount)
    {
        if (isLimited)
        {
            currentAmount += amount;
            if (currentAmount > maxAmount) currentAmount = maxAmount;
            UpdateAmountText(wiggle: true);
        }
    }

    private void UpdateAmountText(bool wiggle = false)
    {
        if (amountText != null)
        {
            amountText.text = currentAmount.ToString();
            if (wiggle)
            {
                // TMP_Text may be a TextMeshProUGUI (Canvas) or TextMeshPro (world)
                var rt = (amountText as TextMeshProUGUI)?.GetComponent<RectTransform>();
                if (rt != null)
                    StartCoroutine(FlavorEffects.Wiggle(rt));
            }
        }
    }

    // ── ISaveable ─────────────────────────────────────────────────────

    public StationSaveRecord CaptureState()
    {
        return new StationSaveRecord
        {
            scenePath   = WorldStateSaver.GetScenePath(gameObject),
            itemType    = ItemType.None,
            itemCount   = 0,
            stockAmount = isLimited ? currentAmount : -1  // -1 = infinite (skip restore)
        };
    }

    public void RestoreState(StationSaveRecord record)
    {
        if (!isLimited || record.stockAmount < 0) return;
        currentAmount = Mathf.Clamp(record.stockAmount, 0, maxAmount);
        UpdateAmountText();
    }
}
