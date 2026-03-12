using UnityEngine;
using TMPro;

public class CustomerWindow : MonoBehaviour, IInteractable
{
    public int currentRequirement = 0;
    private bool hasCustomer = false;

    [Header("UI")]
    public TextMeshProUGUI countText;

    private float spawnTimer = -1f;

    private void Start()
    {
        if (GameManager.Instance.IsServiceTime())
            SpawnCustomer();
        else
            spawnTimer = 2f; // Initial wait once 5AM hits
    }

    private void Update()
    {
        // If day ends, customer leaves
        if (hasCustomer && !GameManager.Instance.isDayActive)
        {
            hasCustomer = false;
            spawnTimer = -1f;
            UpdateUI();
        }

        if (!hasCustomer && GameManager.Instance.isDayActive && GameManager.Instance.IsServiceTime())
        {
            if (spawnTimer < 0) spawnTimer = 2f; // Initialize timer if not set

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnCustomer();
            }
        }
    }

    public void SpawnCustomer()
    {
        if (!GameManager.Instance.IsServiceTime()) return;

        currentRequirement = Random.Range(10, 21);
        hasCustomer = true;
        spawnTimer = -1f;
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
                        GameManager.Instance.AddMoney(payment);
                        Debug.Log("Order completed! Earned: " + payment);
                        
                        Destroy(player.RemoveHeldItem());
                        hasCustomer = false;
                        UpdateUI();
                        
                        // Start spawn timer for next customer
                        spawnTimer = Random.Range(5f, 15f);
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
