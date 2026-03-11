using UnityEngine;

/// <summary>
/// Oven — accepts TrayedShapedDough, bakes it over time, and can burn it.
///
/// State transitions destroy the current prefab and spawn the result prefab
/// in its place, consistent with the per-prefab-per-state pattern.
///
/// Timeline:
///   Insert → wait bakeTime → BakedPandesalTray
///   Wait burnTime total   → BurntPandesalTray
/// </summary>
public class Oven : MonoBehaviour, IInteractable
{
    [Header("Placement")]
    public Transform trayPoint;

    [Header("Timing")]
    public float bakeTime = 10f;
    public float burnTime = 13f;

    [Header("Result Prefabs")]
    [Tooltip("Prefab spawned when baking is complete.")]
    public GameObject bakedPandesalPrefab;

    [Tooltip("Prefab spawned when the tray is left in too long.")]
    public GameObject burntPandesalPrefab;

    // Runtime state
    private GameObject currentTray;
    private float      timer;
    private bool       isBaking;
    private bool       isDone;
    private bool       isBurnt;

    // ---------------------------------------------------------------
    // IInteractable
    // ---------------------------------------------------------------

    public void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            if (currentTray != null) return; // Oven already occupied

            var held = player.GetHeldItem();
            if (held != null)
            {
                var data = held.GetComponentInChildren<ItemData>();
                if (data != null && data.itemType == ItemType.TrayedShapedDough)
                {
                    InsertTray(player);
                }
            }
        }
        else
        {
            // Only allow pickup once baking is done or burnt — not mid-bake
            if (currentTray != null && (isDone || isBurnt))
                RemoveTray(player);
        }
    }

    // ---------------------------------------------------------------
    // Baking timer
    // ---------------------------------------------------------------

    private void Update()
    {
        if (!isBaking) return;

        timer += Time.deltaTime;

        if (!isDone && timer >= bakeTime)
        {
            isDone = true;
            SwapTrayPrefab(bakedPandesalPrefab, "Baking complete — pick up the pandesal!");
        }

        if (!isBurnt && timer >= burnTime)
        {
            isBurnt = true;
            SwapTrayPrefab(burntPandesalPrefab, "Tray burnt!");
        }
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private void InsertTray(PlayerController player)
    {
        currentTray = player.RemoveHeldItem();
        SnapToTrayPoint(currentTray);

        timer    = 0f;
        isBaking = true;
        isDone   = false;
        isBurnt  = false;

        Debug.Log("[OVEN] Tray inserted. Baking started.");
    }

    private void RemoveTray(PlayerController player)
    {
        player.PickUpItem(currentTray);
        currentTray = null;
        isBaking    = false;
        isDone      = false;
        isBurnt     = false;

        Debug.Log("[OVEN] Tray removed by player.");
    }

    /// <summary>
    /// Destroys the current tray object and spawns the result prefab at trayPoint.
    /// </summary>
    private void SwapTrayPrefab(GameObject prefab, string logLabel)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[OVEN] Result prefab not assigned! Assign it in the Inspector. ({logLabel})");
            return;
        }

        Destroy(currentTray);
        currentTray = Instantiate(prefab);
        SnapToTrayPoint(currentTray);

        Debug.Log($"[OVEN] {logLabel}");
    }

    private void SnapToTrayPoint(GameObject obj)
    {
        Counter.AutoFixBadPivots(obj);

        obj.transform.SetParent(trayPoint, true);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // Force physics bodies to stay perfectly still while in the oven
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
    }

    // ---------------------------------------------------------------
    // Prompt text
    // ---------------------------------------------------------------

    public string GetInteractText()
    {
        if (currentTray == null) return "Insert Tray (E)";
        if (isBurnt)  return "Pick Up Burnt Pandesal (E)";
        if (isDone)   return "Pick Up Baked Pandesal (E)";

        float remaining = bakeTime - timer;
        return $"Baking… {remaining:F1}s remaining";
    }
}
