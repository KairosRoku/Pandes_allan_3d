using UnityEngine;

/// <summary>
/// TrashBin — destroys any item the player is holding.
/// </summary>
public class TrashBin : MonoBehaviour, IInteractable
{
    public void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            GameObject held = player.RemoveHeldItem();
            Debug.Log($"[TRASH] Discarded item: {held.name}");
            Destroy(held);
        }
        else
        {
            Debug.Log("[TRASH] Hand is empty. Nothing to discard.");
        }
    }

    public string GetInteractText(PlayerController player)
    {
        return "Discard Item (E)";
    }
}
