using UnityEngine;
using TMPro;

public class DoughBin : MonoBehaviour, IInteractable
{
    public int doughCount = 0;
    public GameObject doughPrefab;

    [Header("UI")]
    public TextMeshProUGUI countText;

    private void Start()
    {
        UpdateUI();
    }

    public void AddDough()
    {
        doughCount++;
        UpdateUI();
        Debug.Log("Dough added! Total: " + doughCount);
    }

    public void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            GameObject held = player.GetHeldItem();
            var data = held.GetComponentInChildren<ItemData>();
            if (data != null && data.itemType == ItemType.Dough)
            {
                doughCount++;
                UpdateUI();
                Destroy(player.RemoveHeldItem());
                Debug.Log("[DOUGH BIN] Player returned a Dough item.");
            }
        }
        else if (doughPrefab != null && doughCount > 0)
        {
            doughCount--;
            UpdateUI();
            GameObject dough = Instantiate(doughPrefab, player.holdPoint.position, player.holdPoint.rotation);
            player.PickUpItem(dough);
        }
    }

    private void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = doughCount.ToString();
        }
    }

    public string GetInteractText(PlayerController player)
    {
        return "Dough Bin (" + doughCount + ")";
    }
}
