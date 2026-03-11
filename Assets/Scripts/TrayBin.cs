using UnityEngine;

/// <summary>
/// TrayBin — an infinite supply of trays.
/// Player (empty-handed) interacts with E to grab one tray.
/// This is auto-interacted when the player touches it (like DoughBin).
/// </summary>
public class TrayBin : MonoBehaviour, IInteractable
{
    [Tooltip("Prefab for the Tray item (must have ItemData with ItemType.Tray)")]
    public GameObject trayPrefab;

    public void Interact(PlayerController player)
    {
        if (trayPrefab != null && !player.IsHoldingItem())
        {
            GameObject tray = Instantiate(trayPrefab, player.holdPoint.position, player.holdPoint.rotation);
            player.PickUpItem(tray);
            Debug.Log("[TRAY BIN] Player picked up a Tray.");
        }
    }

    public string GetInteractText()
    {
        return "Pick Up Tray";
    }
}
