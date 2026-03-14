using UnityEngine;
using TMPro;

public class IngredientRack : MonoBehaviour, IInteractable
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
                    UpdateAmountText();
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
                UpdateAmountText();
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
            UpdateAmountText();
        }
    }

    private void UpdateAmountText()
    {
        if (amountText != null)
        {
            amountText.text = currentAmount.ToString();
        }
    }
}
