using UnityEngine;

/// <summary>
/// Base class for any counter/table in the kitchen.
/// Holds a single item on its itemPlacementPoint.
/// Subclasses override PlaceItem, PickUpItem, and TryHandleSpecialInteraction
/// to add behaviour (e.g. dough processing, tray combining).
/// </summary>
public class Counter : MonoBehaviour, IInteractable
{
    [Tooltip("Where items are placed on this counter surface.")]
    public Transform itemPlacementPoint;

    /// <summary>The item currently sitting on this counter. Null if empty.</summary>
    protected GameObject itemOnCounter;

    // ---------------------------------------------------------------
    // IInteractable
    // ---------------------------------------------------------------

    public virtual void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            if (itemOnCounter == null)
                PlaceItem(player);
            else
                TryHandleSpecialInteraction(player);
        }
        else
        {
            if (itemOnCounter != null)
                PickUpItem(player);
        }
    }

    protected virtual void Start()
    {
        // AUTO-FIX: If the user accidentally moved the itemPlacementPoint way off the table,
        // snap it perfectly to the top-center surface of the physical table mesh.
        if (itemPlacementPoint != null)
        {
            Renderer[] r = GetComponentsInChildren<Renderer>();
            if (r.Length > 0)
            {
                Bounds b = r[0].bounds;
                for (int i = 1; i < r.Length; i++)
                {
                    if (!r[i].transform.IsChildOf(itemPlacementPoint))
                        b.Encapsulate(r[i].bounds);
                }
                
                Vector3 properTop = b.center;
                properTop.y = b.max.y; // The very top surface
                
                // If it's more than half a meter off from the visual top center, fix it
                if (Vector3.Distance(itemPlacementPoint.position, properTop) > 0.5f)
                {
                    Debug.LogWarning($"[COUNTER] Auto-fixed {gameObject.name}'s itemPlacementPoint because it was too far off-center.");
                    itemPlacementPoint.position = properTop;
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // Core helpers (overridable)
    // ---------------------------------------------------------------

    protected virtual void PlaceItem(PlayerController player)
    {
        if (itemOnCounter != null) return;

        itemOnCounter = player.RemoveHeldItem();
        SnapToPlacementPoint(itemOnCounter);

        Debug.Log($"[COUNTER] Placed '{itemOnCounter.name}' on '{gameObject.name}'.");
    }

    protected virtual void PickUpItem(PlayerController player)
    {
        if (itemOnCounter == null) return;

        player.PickUpItem(itemOnCounter);
        itemOnCounter = null;

        Debug.Log($"[COUNTER] Item picked up from '{gameObject.name}'.");
    }

    /// <summary>
    /// Called when the player interacts while holding an item and the counter
    /// already has something on it. Override to handle combining/swapping.
    /// </summary>
    protected virtual void TryHandleSpecialInteraction(PlayerController player)
    {
        if (itemOnCounter == null) return;
        var tableData = itemOnCounter.GetComponentInChildren<ItemData>();
        
        var held = player.GetHeldItem();
        if (held == null) return;
        var heldData = held.GetComponentInChildren<ItemData>();

        if (tableData != null && heldData != null)
        {
            // Packaging logic: Holding PaperBag, table has BakedPandesalTray
            if (heldData.itemType == ItemType.PaperBag && tableData.itemType == ItemType.BakedPandesalTray)
            {
                if (PackingMinigameUI.Instance != null)
                {
                    PackingMinigameUI.Instance.OpenMinigame(player, tableData, heldData);
                }
                else
                {
                    // Fallback if UI is not set up
                    heldData.count += tableData.count;
                    tableData.count = 0;
                    Debug.Log($"[PACKAGING] Instant pack fallback (UI missing). Bag: {heldData.count}");
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // Shared utility
    // ---------------------------------------------------------------

    protected void SnapToPlacementPoint(GameObject obj)
    {
        AutoFixBadPivots(obj); // Force the prefabs' visual mesh to center on their roots

        obj.transform.SetParent(itemPlacementPoint, true);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // Force physics bodies to stay perfectly still while on the counter
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }

        // Force all colliders to become triggers while on the counter
        // This stops instantly-spawned prefabs from violently pushing the player
        var colliders = obj.GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
        {
            c.isTrigger = true;
        }
    }

    /// <summary>
    /// Matches the true visual bounds (mesh center) of an object with its root transform position.
    /// This fixes illusions where the user's Prefab had its 3D model shifted far to the side of the 0,0,0 root.
    /// </summary>
    public static void AutoFixBadPivots(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }
            
            Vector3 offset = obj.transform.position - b.center;
            
            // If the visual center is off by more than 5cm, mathematically slide the children to the root
            if (offset.magnitude > 0.05f)
            {
                foreach (Transform child in obj.transform)
                {
                    child.position += offset;
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // IInteractable
    // ---------------------------------------------------------------

    public virtual string GetInteractText(PlayerController player)
    {
        if (itemOnCounter != null)
        {
            var data = itemOnCounter.GetComponentInChildren<ItemData>();
            if (data != null)
            {
                string info = "";
                if (data.itemType == ItemType.BakedPandesalTray || data.itemType == ItemType.PaperBag)
                    info = $" ({data.count} pcs)";

                if (data.itemType == ItemType.BakedPandesalTray)
                {
                    return $"Pick Up {itemOnCounter.name}{info} (E) | Pack with Bag (E)";
                }
                return $"Pick Up {itemOnCounter.name}{info} (E)";
            }
            return $"Pick Up {itemOnCounter.name} (E)";
        }
        return "Place Item (E)";
    }
}
