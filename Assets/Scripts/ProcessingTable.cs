using UnityEngine;

public class ProcessingTable : Counter
{
    public GameObject doughKneadPrefab;

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
                // Check if we can process it (Direct interactions for empty hands)
                if (itemsSlot.TryGetComponent<ItemData>(out var data))
                {
                    if (data.itemType == ItemType.Dough)
                    {
                        TransformDough(ItemType.RolledDough);
                        Debug.Log("[PROCESSING TABLE] Dough -> Rolled (dough shape)");
                        return; // Done
                    }
                    else if (data.itemType == ItemType.RolledDough)
                    {
                        TransformDough(ItemType.ShapedDough);
                        Debug.Log("[PROCESSING TABLE] Rolled -> Shaped");
                        return; // Done
                    }
                }
                
                // If no special interaction, pick it up
                PickUpItem(player);
            }
        }
    }

    protected override void PlaceItem(PlayerController player)
    {
        if (itemsSlot != null) return;

        GameObject held = player.GetHeldItem();
        if (held != null && held.TryGetComponent<ItemData>(out var data))
        {
            // Specifically handling placing a raw dough lump to transform it into the processing prefab
            if (data.itemType == ItemType.Dough && doughKneadPrefab != null)
            {
                Destroy(player.RemoveHeldItem()); // Consume the raw inventory item
                itemsSlot = Instantiate(doughKneadPrefab, itemPlacementPoint);
                itemsSlot.transform.localPosition = Vector3.up * 0.01f; 
                itemsSlot.transform.localRotation = Quaternion.identity;
                itemsSlot.transform.localScale = Vector3.one; 
                
                // Ensure the spawned object is the right type
                if (itemsSlot.TryGetComponent<ItemData>(out var newData))
                {
                    newData.itemType = ItemType.Dough;
                }
                
                UpdateVisuals(ItemType.Dough);
                Debug.Log("[PROCESSING TABLE] Placed raw dough; spawned knead prefab.");
                return;
            }
        }

        base.PlaceItem(player);
    }

    protected override void TryHandleSpecialInteraction(PlayerController player)
    {
        // Handle Tray combination
        GameObject held = player.GetHeldItem();
        if (held != null && held.TryGetComponent<ItemData>(out var heldData))
        {
            if (heldData.itemType == ItemType.Tray)
            {
                if (itemsSlot != null && itemsSlot.TryGetComponent<ItemData>(out var tableData))
                {
                    if (tableData.itemType == ItemType.ShapedDough)
                    {
                        // Combine into Trayed Shaped Dough
                        // IMPORTANT: RemoveHeldItem first (detaches from player) then Destroy
                        GameObject trayToDestroy = player.RemoveHeldItem();
                        Destroy(trayToDestroy);
                        
                        // Transform the dough on the table into a tray-carrying version
                        TransformDough(ItemType.TrayedShapedDough);
                        Debug.Log("[PROCESSING TABLE] Added Tray to Shaped Dough -> TrayedShapedDough!");
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
            UpdateVisuals(newType);
        }
    }

    private void UpdateVisuals(ItemType type)
    {
        if (itemsSlot != null && itemsSlot.TryGetComponent<DoughVisuals>(out var visuals))
        {
            visuals.RefreshVisuals();
        }
    }

    public override string GetInteractText()
    {
        if (itemsSlot != null && itemsSlot.TryGetComponent<ItemData>(out var data))
        {
            if (data.itemType == ItemType.Dough) return "Knead the Dough";
            if (data.itemType == ItemType.RolledDough) return "Shape the Dough";
            if (data.itemType == ItemType.ShapedDough) return "Bring a Tray to load it";
            if (data.itemType == ItemType.TrayedShapedDough) return "Pick Up Trayed Dough (Oven Ready!)";
        }
        return base.GetInteractText();
    }
}
