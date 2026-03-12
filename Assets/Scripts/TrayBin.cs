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
        if (player.IsHoldingItem())
        {
            GameObject held = player.GetHeldItem();
            var data = held.GetComponentInChildren<ItemData>();
            if (data != null && data.itemType == ItemType.Tray)
            {
                Destroy(player.RemoveHeldItem());
                Debug.Log("[TRAY BIN] Player returned a Tray.");
            }
        }
        else if (trayPrefab != null)
        {
            GameObject tray = Instantiate(trayPrefab, player.holdPoint.position, player.holdPoint.rotation);
            player.PickUpItem(tray);
            Debug.Log("[TRAY BIN] Player picked up a Tray.");
        }
    }

    public string GetInteractText(PlayerController player)
    {
        return "Pick Up Tray";
    }
}
