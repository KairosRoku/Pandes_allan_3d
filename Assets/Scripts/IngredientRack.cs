using UnityEngine;

public class IngredientRack : MonoBehaviour, IInteractable
{
    public GameObject ingredientPrefab;
    public ItemType itemType;

    public void Interact(PlayerController player)
    {
        if (ingredientPrefab != null && !player.IsHoldingItem())
        {
            GameObject newIngredient = Instantiate(ingredientPrefab, player.holdPoint.position, player.holdPoint.rotation);
            player.PickUpItem(newIngredient);
        }
    }

    public string GetInteractText()
    {
        return "Pick Up " + itemType.ToString();
    }
}
