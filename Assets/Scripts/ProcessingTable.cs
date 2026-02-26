using UnityEngine;

public class ProcessingTable : Counter
{
    public MinigameManager minigameManager;

    public override void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            if (itemsSlot == null)
            {
                PlaceItem(player);
            }
            else
            {
                TryHandleSpecialInteraction(player);
            }
        }
        else
        {
            if (itemsSlot != null)
            {
                // Check if we can process it
                if (itemsSlot.TryGetComponent<ItemData>(out var data))
                {
                    if (data.itemType == ItemType.Dough)
                    {
                        // Start Rolling Minigame
                        minigameManager.StartRollingMinigame(this);
                    }
                    else if (data.itemType == ItemType.RolledDough)
                    {
                        // Start Shaping Minigame
                        minigameManager.StartShapingMinigame(this);
                    }
                    else if (data.itemType == ItemType.ShapedDough)
                    {
                        // Tray interaction handled in TryHandleSpecialInteraction
                        PickUpItem(player);
                    }
                    else
                    {
                        PickUpItem(player);
                    }
                }
            }
        }
    }

    protected override void TryHandleSpecialInteraction(PlayerController player)
    {
        // Handle Tray addition
        GameObject held = player.GetHeldItem();
        if (held != null && held.TryGetComponent<ItemData>(out var heldData))
        {
            if (heldData.itemType == ItemType.Tray)
            {
                if (itemsSlot.TryGetComponent<ItemData>(out var tableData))
                {
                    if (tableData.itemType == ItemType.ShapedDough)
                    {
                        // Combine into Trayed Shaped Dough
                        Destroy(held);
                        player.RemoveHeldItem(); // Consume the tray from hand
                        
                        // Transform dough on table
                        TransformDough(ItemType.TrayedShapedDough);
                    }
                }
            }
        }
    }

    public void TransformDough(ItemType newType)
    {
        if (itemsSlot == null) return;
        
        if (itemsSlot.TryGetComponent<ItemData>(out var data))
        {
            data.itemType = newType;
            // Here you would also change the visual prefab/model
            UpdateVisuals(newType);
        }
    }

    private void UpdateVisuals(ItemType type)
    {
        if (itemsSlot != null && itemsSlot.TryGetComponent<DoughVisuals>(out var visuals))
        {
            visuals.RefreshVisuals();
        }
        else
        {
            Debug.Log("Dough transformed to " + type + " but no DoughVisuals found.");
        }
    }

    public override string GetInteractText()
    {
        if (itemsSlot != null)
        {
            if (itemsSlot.TryGetComponent<ItemData>(out var data))
            {
                if (data.itemType == ItemType.Dough) return "Interact to Roll";
                if (data.itemType == ItemType.RolledDough) return "Interact to Shape";
                if (data.itemType == ItemType.ShapedDough) return "Needs Tray (or pick up)";
            }
        }
        return base.GetInteractText();
    }
}
