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
                        if (SwapPrefab(doughKneadPrefab, ItemType.DoughKnead, "Dough → Dough Knead")) return;
                        break; // If swap fails (e.g. missing prefab), fall through to pick up

                    case ItemType.DoughKnead:
                        if (SwapPrefab(shapedDoughPrefab, ItemType.ShapedDough, "Dough Knead → Shaped Dough")) return;
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

        if (tableData.itemType != ItemType.ShapedDough)
        {
            base.TryHandleSpecialInteraction(player);
            return;
        }

        GameObject held = player.GetHeldItem();
        if (held == null) return;
        var heldData = held.GetComponentInChildren<ItemData>();
        if (heldData == null || heldData.itemType != ItemType.Tray)
        {
            base.TryHandleSpecialInteraction(player);
            return;
        }

        // Consume the tray, swap to trayed prefab
        Destroy(player.RemoveHeldItem());
        SwapPrefab(trayedShapedDoughPrefab, ItemType.TrayedShapedDough, "Shaped Dough + Tray → Trayed Shaped Dough");
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

    public override string GetInteractText()
    {
        if (itemOnCounter != null)
        {
            var data = itemOnCounter.GetComponentInChildren<ItemData>();
            if (data != null)
            {
                return data.itemType switch
                {
                    ItemType.Dough             => "Knead the Dough (E)",
                    ItemType.DoughKnead        => "Shape the Dough (E)",
                    ItemType.ShapedDough       => "Bring a Tray to load it (E)",
                    ItemType.TrayedShapedDough => "Pick Up — Oven Ready! (E)",
                    ItemType.BakedPandesalTray => "Pick Up (E) | Pack with Bag (E)",
                    _                          => "Pick Up (E)"
                };
            }
        }

        return "Place Item (E)";
    }
}
