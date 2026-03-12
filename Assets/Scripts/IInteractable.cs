using UnityEngine;

public interface IInteractable
{
    void Interact(PlayerController player);
    string GetInteractText(PlayerController player);
}

public interface IPickable
{
    ItemType GetItemType();
    GameObject PickUp();
    void Drop();
}
