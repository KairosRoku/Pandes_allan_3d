using UnityEngine;
using TMPro;

public class CustomerWindow : MonoBehaviour, IInteractable
{
    public int currentRequirement = 0;
    private bool hasCustomer = false;

    [Header("UI")]
    public TextMeshProUGUI countText;

    private void Start()
    {
        SpawnCustomer();
    }

    public void SpawnCustomer()
    {
        currentRequirement = Random.Range(10, 21);
        hasCustomer = true;
        UpdateUI();
        Debug.Log("Customer wants " + currentRequirement + " pandesals!");
    }

    public void Interact(PlayerController player)
    {
        if (hasCustomer && player.IsHoldingItem())
        {
            GameObject held = player.GetHeldItem();
            if (held != null)
            {
                var data = held.GetComponentInChildren<ItemData>();
                if (data != null && data.itemType == ItemType.PaperBag)
                {
                    if (data.count >= currentRequirement)
                    {
                        // Success!
                        int payment = currentRequirement * 5; // Example price
                        GameManager.Instance.money += payment;
                        Debug.Log("Order completed! Earned: " + payment);
                        
                        Destroy(player.RemoveHeldItem());
                        hasCustomer = false;
                        UpdateUI();
                        Invoke("SpawnCustomer", Random.Range(5f, 15f));
                    }
                    else
                    {
                        Debug.Log("Not enough pandesals in the bag! Need " + currentRequirement);
                    }
                }
            }
        }
    }

    private void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = hasCustomer ? currentRequirement.ToString() : "";
        }
    }

    public string GetInteractText(PlayerController player)
    {
        if (!hasCustomer) return "Waiting for Customer...";
        return "Customer wants " + currentRequirement + " pandesals";
    }
}
