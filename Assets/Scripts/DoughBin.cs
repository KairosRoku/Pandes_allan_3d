using UnityEngine;

public class DoughBin : MonoBehaviour, IInteractable
{
    public int doughCount = 0;
    public GameObject doughPrefab;

    public void AddDough()
    {
        doughCount++;
        Debug.Log("Dough added! Total: " + doughCount);
    }

    public void Interact(PlayerController player)
    {
        if (doughPrefab != null && !player.IsHoldingItem() && doughCount > 0)
        {
            doughCount--;
            GameObject dough = Instantiate(doughPrefab, player.holdPoint.position, player.holdPoint.rotation);
            player.PickUpItem(dough);
        }
    }

    public string GetInteractText()
    {
        return "Dough Bin (" + doughCount + ")";
    }
}
