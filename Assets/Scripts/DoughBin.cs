using UnityEngine;
using TMPro;

public class DoughBin : MonoBehaviour, IInteractable, ISaveable
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
        UpdateUI(wiggle: true);
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
                UpdateUI(wiggle: true);
                Destroy(player.RemoveHeldItem());
                Debug.Log("[DOUGH BIN] Player returned a Dough item.");
            }
        }
        else if (doughPrefab != null && doughCount > 0)
        {
            doughCount--;
            UpdateUI(wiggle: true);
            GameObject dough = Instantiate(doughPrefab, player.holdPoint.position, player.holdPoint.rotation);
            player.PickUpItem(dough);
        }
    }

    private void UpdateUI(bool wiggle = false)
    {
        if (countText != null)
        {
            countText.text = doughCount.ToString();
            if (wiggle)
            {
                var rt = countText.GetComponent<RectTransform>();
                if (rt != null)
                    StartCoroutine(FlavorEffects.Wiggle(rt));
            }
        }
    }

    public string GetInteractText(PlayerController player)
    {
        return "Dough Bin (" + doughCount + ")";
    }

    // ── ISaveable ─────────────────────────────────────────────────────

    public StationSaveRecord CaptureState()
    {
        return new StationSaveRecord
        {
            scenePath   = WorldStateSaver.GetScenePath(gameObject),
            itemType    = ItemType.None,   // DoughBin tracks stock, not a placed item
            itemCount   = 0,
            stockAmount = doughCount
        };
    }

    public void RestoreState(StationSaveRecord record)
    {
        if (record.stockAmount < 0) return;
        doughCount = record.stockAmount;
        UpdateUI();
    }
}
