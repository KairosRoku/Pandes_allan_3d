using UnityEngine;

public class Counter : MonoBehaviour, IInteractable
{
    public Transform itemPlacementPoint;
    protected GameObject itemsSlot;

    public virtual void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            if (itemsSlot == null)
            {
                PlaceItem(player);
            }
            else
            {
                // Try to swap or combine (to be implemented in specific classes)
                TryHandleSpecialInteraction(player);
            }
        }
        else
        {
            if (itemsSlot != null)
            {
                PickUpItem(player);
            }
        }
    }

    protected virtual void PlaceItem(PlayerController player)
    {
        if (itemsSlot != null) return; // Already occupied

        itemsSlot = player.RemoveHeldItem();
        itemsSlot.transform.SetParent(itemPlacementPoint);
        itemsSlot.transform.localPosition = Vector3.zero;
        itemsSlot.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"[COUNTER] Item {itemsSlot.name} placed at {itemPlacementPoint.name}. Counter is now OCCUPIED.");
    }

    protected virtual void PickUpItem(PlayerController player)
    {
        if (itemsSlot == null) return;

        player.PickUpItem(itemsSlot);
        itemsSlot = null;
        
        Debug.Log("[COUNTER] Item picked up. Counter is now FREE.");
    }

    protected virtual void TryHandleSpecialInteraction(PlayerController player)
    {
        // Default: do nothing if occupied
    }

    public virtual string GetInteractText()
    {
        if (itemsSlot != null) return "Pick Up " + itemsSlot.name;
        return "Place Item";
    }
}
