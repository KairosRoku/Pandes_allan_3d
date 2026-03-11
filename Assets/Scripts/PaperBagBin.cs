using UnityEngine;

/// <summary>
/// Infinite supply of Paper Bags.
/// Auto-interacted when empty-handed.
/// </summary>
public class PaperBagBin : MonoBehaviour, IInteractable
{
    [Tooltip("Prefab for the PaperBag item (must have ItemData with ItemType.PaperBag)")]
    public GameObject paperBagPrefab;

    public void Interact(PlayerController player)
    {
        if (paperBagPrefab != null && !player.IsHoldingItem())
        {
            GameObject bag = Instantiate(paperBagPrefab);
            
            var data = bag.GetComponentInChildren<ItemData>();
            if (data != null)
            {
                data.itemType = ItemType.PaperBag;
                data.count = 0; // The bag starts empty
            }

            player.PickUpItem(bag);
            Debug.Log("[PAPER BAG BIN] Player picked up an empty Paper Bag.");
        }
    }

    public string GetInteractText()
    {
        return "Pick Up Paper Bag";
    }
}
