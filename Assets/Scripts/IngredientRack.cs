using UnityEngine;

public class IngredientRack : MonoBehaviour, IInteractable
{
    public GameObject ingredientPrefab;
    public ItemType itemType;

    public void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            GameObject held = player.GetHeldItem();
            var data = held.GetComponentInChildren<ItemData>();
            if (data != null && data.itemType == this.itemType)
            {
                Destroy(player.RemoveHeldItem());
                Debug.Log($"[INGREDIENT RACK] Player returned {itemType}.");
            }
        }
        else if (ingredientPrefab != null)
        {
            GameObject newIngredient = Instantiate(ingredientPrefab, player.holdPoint.position, player.holdPoint.rotation);
            player.PickUpItem(newIngredient);
        }
    }

    public string GetInteractText(PlayerController player)
    {
        return "Pick Up " + itemType.ToString();
    }
}
