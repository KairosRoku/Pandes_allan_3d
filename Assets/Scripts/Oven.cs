using UnityEngine;
using System.Collections;

public class Oven : MonoBehaviour, IInteractable
{
    public Transform trayPoint;
    public GameObject bakedPrefab;
    public GameObject burntPrefab;

    private GameObject currentTray;
    private float timer = 0f;
    private bool isBaking = false;
    private bool isBurnt = false;
    private bool isDone = false;

    public void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            if (currentTray == null)
            {
                var held = player.GetHeldItem();
                if (held.TryGetComponent<ItemData>(out var data) && data.itemType == ItemType.TrayedShapedDough)
                {
                    PlaceTray(player);
                }
            }
        }
        else
        {
            if (currentTray != null)
            {
                PickUpTray(player);
            }
        }
    }

    private void PlaceTray(PlayerController player)
    {
        currentTray = player.RemoveHeldItem();
        currentTray.transform.SetParent(trayPoint);
        currentTray.transform.localPosition = Vector3.zero;
        currentTray.transform.localRotation = Quaternion.identity;
        
        timer = 0f;
        isBaking = true;
        isDone = false;
        isBurnt = false;
    }

    private void PickUpTray(PlayerController player)
    {
        player.PickUpItem(currentTray);
        currentTray = null;
        isBaking = false;
        isDone = false;
        isBurnt = false;
    }

    private void Update()
    {
        if (isBaking)
        {
            timer += Time.deltaTime;

            if (timer >= 10f && !isDone)
            {
                isDone = true;
                // Transition to Baked
                SetTrayState(ItemType.BakedPandesalTray);
            }

            if (timer >= 13f && !isBurnt)
            {
                isBurnt = true;
                // Transition to Burnt
                SetTrayState(ItemType.BurntPandesalTray);
            }
        }
    }

    private void SetTrayState(ItemType type)
    {
        if (currentTray == null) return;
        if (currentTray.TryGetComponent<ItemData>(out var data))
        {
            data.itemType = type;
            // Visual Update...
            if (currentTray.TryGetComponent<DoughVisuals>(out var visuals))
            {
                visuals.RefreshVisuals();
            }
            Debug.Log("Oven: Tray is now " + type);
        }
    }

    public string GetInteractText()
    {
        if (currentTray == null) return "Insert Tray";
        if (isBurnt) return "Pick Up Burnt Pandesal!";
        if (isDone) return "Pick Up Baked Pandesal!";
        return "Baking... " + (10f - timer).ToString("F1") + "s";
    }
}
