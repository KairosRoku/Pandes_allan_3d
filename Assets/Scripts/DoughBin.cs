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
        if (doughPrefab != null && !player.IsHoldingItem() && doughCount > 0)
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
