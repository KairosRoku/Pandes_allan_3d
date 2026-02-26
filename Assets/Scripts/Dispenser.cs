using UnityEngine;

public class Dispenser : MonoBehaviour, IInteractable
{
    public GameObject prefab;
    public ItemType itemType;

    public void Interact(PlayerController player)
    {
        if (prefab != null && !player.IsHoldingItem())
        {
            GameObject obj = Instantiate(prefab, player.holdPoint.position, player.holdPoint.rotation);
            player.PickUpItem(obj);
        }
    }

    public string GetInteractText()
    {
        return "Pick Up " + itemType.ToString();
    }
}
