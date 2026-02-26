using UnityEngine;

public class PackagingStation : Counter
{
    public MinigameManager minigameManager;

    public override void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            GameObject held = player.GetHeldItem();
            if (held.TryGetComponent<ItemData>(out var heldData))
            {
                if (heldData.itemType == ItemType.PaperBag)
                {
                    if (itemsSlot != null && itemsSlot.TryGetComponent<ItemData>(out var tableData))
                    {
                        if (tableData.itemType == ItemType.BakedPandesalTray)
                        {
                            // Start Packaging Minigame
                            minigameManager.StartPackagingMinigame(this, player);
                            return;
                        }
                    }
                }
            }
        }
        
        base.Interact(player);
    }

    public override string GetInteractText()
    {
        if (itemsSlot != null && itemsSlot.TryGetComponent<ItemData>(out var data))
        {
            if (data.itemType == ItemType.BakedPandesalTray)
            {
                return "Need Paper Bag to Pack";
            }
        }
        return base.GetInteractText();
    }
}
