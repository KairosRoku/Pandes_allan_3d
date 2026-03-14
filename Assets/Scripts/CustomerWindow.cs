using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CustomerWindow : MonoBehaviour, IInteractable
{
    [Header("Queue System")]
    public GameObject customerPrefab;
    public Transform[] queueSpots; // Assign positions for customers in line
    public int maxQueueSize = 3;
    
    private List<Customer> customerQueue = new List<Customer>();
    private float spawnTimer = -1f;

    [Header("UI")]
    public TextMeshProUGUI countText;

    private void Start()
    {
        if (GameManager.Instance.IsServiceTime())
            TrySpawnCustomer();
        else
            spawnTimer = 2f;
    }

    private void Update()
    {
        // If day ends, everyone leaves
        if (!GameManager.Instance.isDayActive && customerQueue.Count > 0)
        {
            ClearQueue();
        }

        if (GameManager.Instance.isDayActive && GameManager.Instance.IsServiceTime())
        {
            if (customerQueue.Count < maxQueueSize)
            {
                if (spawnTimer < 0)
                {
                    float currentHour = GameManager.Instance.GetCurrentHour();
                    if (currentHour >= 5 && currentHour <= 8)
                    {
                        // Peak hours: Fast spawning
                        spawnTimer = Random.Range(3f, 7f);
                    }
                    else
                    {
                        // Normal hours: Slower spawning
                        spawnTimer = Random.Range(10f, 20f);
                    }
                }

                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0)
                {
                    TrySpawnCustomer();
                    spawnTimer = -1f; // Force recalculation next frame
                }
            }
        }
    }

    private void TrySpawnCustomer()
    {
        if (customerQueue.Count >= maxQueueSize || !GameManager.Instance.IsServiceTime()) return;

        GameObject obj = Instantiate(customerPrefab);
        Customer c = obj.GetComponent<Customer>();
        
        int req = Random.Range(5, 16);
        c.Initialize(this, req);
        
        customerQueue.Add(c);
        UpdateQueuePositions();
        
        // If this is the first customer, start their timer
        if (customerQueue.Count == 1)
        {
            customerQueue[0].StartServing();
        }

        UpdateUI();
    }

    private void UpdateQueuePositions()
    {
        for (int i = 0; i < customerQueue.Count; i++)
        {
            if (i < queueSpots.Length)
            {
                customerQueue[i].transform.position = queueSpots[i].position;
                customerQueue[i].transform.rotation = queueSpots[i].rotation;
            }
        }
    }

    public void OnCustomerLeft(Customer c, bool satisfied)
    {
        if (customerQueue.Contains(c))
        {
            customerQueue.Remove(c);
            Destroy(c.gameObject);
            
            UpdateQueuePositions();
            
            // Start serving the next person in line
            if (customerQueue.Count > 0)
            {
                customerQueue[0].StartServing();
            }
            
            UpdateUI();
        }
    }

    public void Interact(PlayerController player)
    {
        if (customerQueue.Count > 0 && player.IsHoldingItem())
        {
            Customer current = customerQueue[0];
            GameObject held = player.GetHeldItem();
            if (held != null)
            {
                var data = held.GetComponentInChildren<ItemData>();
                if (data != null && data.itemType == ItemType.PaperBag)
                {
                    if (data.count >= current.pandesalRequirement)
                    {
                        // Success!
                        int payment = current.pandesalRequirement * 2;
                        GameManager.Instance.AddMoney(payment);
                        Debug.Log("[SERVICE] Order completed!");

                        Destroy(player.RemoveHeldItem());
                        current.LeaveSatisfied();
                    }
                    else
                    {
                        Debug.Log($"[SERVICE] Not enough pandesals! Need {current.pandesalRequirement}");
                    }
                }
            }
        }
    }

    private void ClearQueue()
    {
        foreach (var c in customerQueue)
        {
            if (c != null) Destroy(c.gameObject);
        }
        customerQueue.Clear();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (countText != null)
        {
            if (customerQueue.Count > 0)
                countText.text = customerQueue[0].pandesalRequirement.ToString();
            else
                countText.text = "";
        }
    }

    public string GetInteractText(PlayerController player)
    {
        if (customerQueue.Count == 0) return "Waiting for Customer...";
        return "Serve Customer (" + customerQueue[0].pandesalRequirement + ")";
    }
}
