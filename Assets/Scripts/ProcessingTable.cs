using UnityEngine;

/// <summary>
/// Processing Table — handles all dough transformation steps.
///
/// Each state transition DESTROYS the current prefab and INSTANTIATES the next
/// one in its place. Each dough state is a standalone prefab with its own model.
///
///   Dough  ──(E)──►  DoughKnead  ──(E)──►  ShapedDough
///   ShapedDough + Tray  ──(E)──►  TrayedShapedDough  ──► ready for Oven
/// </summary>
public class ProcessingTable : Counter
{
    [Header("Dough Stage Prefabs")]
    [Tooltip("Prefab spawned after kneading raw dough.")]
    public GameObject doughKneadPrefab;

    [Tooltip("Prefab spawned after shaping kneaded dough.")]
    public GameObject shapedDoughPrefab;

    [Tooltip("Prefab spawned after placing shaped dough on a tray.")]
    public GameObject trayedShapedDoughPrefab;

    // ---------------------------------------------------------------
    // Counter overrides
    // ---------------------------------------------------------------

    public override void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            if (itemOnCounter == null)
                PlaceItem(player);
            else
                TryHandleSpecialInteraction(player); // e.g. Tray combining
        }
        else
        {
            if (itemOnCounter == null) return;

            var data = itemOnCounter.GetComponentInChildren<ItemData>();
            if (data != null)
            {
                switch (data.itemType)
                {
                    case ItemType.Dough:
                        if (KneadingMinigameUI.Instance != null)
                        {
                            KneadingMinigameUI.Instance.StartMinigame(player, () => {
                                SwapPrefab(doughKneadPrefab, ItemType.DoughKnead, "Dough → Dough Knead");
                            });
                            return;
                        }
                        else
                        {
                            if (SwapPrefab(doughKneadPrefab, ItemType.DoughKnead, "Dough → Dough Knead")) return;
                        }
                        break;

                    case ItemType.DoughKnead:
                        if (ShapingMinigameUI.Instance != null)
                        {
                            ShapingMinigameUI.Instance.StartMinigame(player, () => {
                                SwapPrefab(shapedDoughPrefab, ItemType.ShapedDough, "Dough Knead → Shaped Dough");
                            });
                            return;
                        }
                        else
                        {
                            if (SwapPrefab(shapedDoughPrefab, ItemType.ShapedDough, "Dough Knead → Shaped Dough")) return;
                        }
                        break;
                }
            }

            // Default: pick up whatever is on the table
            PickUpItem(player);
        }
    }

    // ---------------------------------------------------------------
    // Tray combination
    // ---------------------------------------------------------------

    protected override void TryHandleSpecialInteraction(PlayerController player)
    {
        if (itemOnCounter == null) return;
        var tableData = itemOnCounter.GetComponentInChildren<ItemData>();
        if (tableData == null) return;

        GameObject held = player.GetHeldItem();
        if (held == null) return;
        var heldData = held.GetComponentInChildren<ItemData>();
        if (heldData == null) return;

        // Case 1: Table has ShapedDough, Player holds Tray
        bool case1 = (tableData.itemType == ItemType.ShapedDough && heldData.itemType == ItemType.Tray);
        
        // Case 2: Table has Tray, Player holds ShapedDough
        bool case2 = (tableData.itemType == ItemType.Tray && heldData.itemType == ItemType.ShapedDough);

        if (case1 || case2)
        {
            // Consume the held item
            Destroy(player.RemoveHeldItem());
            // Transform the table item
            SwapPrefab(trayedShapedDoughPrefab, ItemType.TrayedShapedDough, "Combining Shaped Dough + Tray → Trayed Shaped Dough");
            return;
        }

        base.TryHandleSpecialInteraction(player);
    }

    // ---------------------------------------------------------------
    // Prefab swap helper
    // ---------------------------------------------------------------

    private bool SwapPrefab(GameObject prefab, ItemType expectedType, string logLabel)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[PROCESSING TABLE] Missing prefab for step: {logLabel}. Assign it in the Inspector.");
            return false;
        }

        if (prefab.GetComponent<Counter>() != null)
        {
            Debug.LogError($"[CRITICAL ASSIGNMENT ERROR] You accidentally assigned a Table/Counter to a Dough Prefab slot! The prefab '{prefab.name}' has a Counter script on it. Please assign the standalone dough mesh prefab instead.");
            return false;
        }

        Destroy(itemOnCounter);
        itemOnCounter = Instantiate(prefab);
        SnapToPlacementPoint(itemOnCounter);

        // Force the type to ensure the state machine advances even if the 
        // user's assigned prefab had the wrong ItemType in the inspector.
        var data = itemOnCounter.GetComponentInChildren<ItemData>();
        if (data != null) data.itemType = expectedType;

        Debug.Log($"[PROCESSING TABLE] {logLabel}");
        return true;
    }

    // ---------------------------------------------------------------
    // Prompt text
    // ---------------------------------------------------------------

    public override string GetInteractText(PlayerController player)
    {
        if (itemOnCounter != null)
        {
            var data = itemOnCounter.GetComponentInChildren<ItemData>();
            if (data != null)
            {
                string info = (data.itemType == ItemType.BakedPandesalTray) ? $" ({data.count} pcs)" : "";

                return data.itemType switch
                {
                    ItemType.Dough             => "Knead the Dough (E)",
                    ItemType.DoughKnead        => "Shape the Dough (E)",
                    ItemType.ShapedDough       => player.IsHoldingItem() && player.GetHeldItem().GetComponentInChildren<ItemData>()?.itemType == ItemType.Tray 
                                                  ? "Add Tray (E)" : "Bring a Tray to load it (E)",
                    ItemType.Tray              => player.IsHoldingItem() && player.GetHeldItem().GetComponentInChildren<ItemData>()?.itemType == ItemType.ShapedDough 
                                                  ? "Add Shaped Dough (E)" : "Pick Up Tray (E)",
                    ItemType.TrayedShapedDough => "Pick Up — Oven Ready! (E)",
                    ItemType.BakedPandesalTray => $"Pick Up{info} (E) | Pack with Bag (E)",
                    _                          => $"Pick Up {itemOnCounter.name}{info} (E)"
                };
            }
        }

        return "Place Item (E)";
    }
}
