using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DoughMaker3000 : MonoBehaviour, IInteractable
{
    public DoughBin doughBin;
    public GameObject doughItemPrefab;

    [Header("UI Indicators")]
    public GameObject flourIndicator;
    public GameObject sugarIndicator;
    public GameObject waterIndicator;

    [Header("Mixing Timing")]
    public float mixingTime = 5f;
    private float mixingTimer;
    private bool isMixing;

    [Header("Mixing UI")]
    public GameObject timerCanvas;
    public Image timerFillImage;

    private bool hasFlour;
    private bool hasSugar;
    private bool hasWater;

    private void Start()
    {
        if (timerCanvas != null) timerCanvas.SetActive(false);
        UpdateUI();
    }

    private void Update()
    {
        if (isMixing)
        {
            mixingTimer += Time.deltaTime;
            
            if (timerFillImage != null)
                timerFillImage.fillAmount = Mathf.Clamp01(mixingTimer / mixingTime);

            if (mixingTimer >= mixingTime)
            {
                FinishMixing();
            }
        }
    }

    public void Interact(PlayerController player)
    {
        if (isMixing) return; // Busy mixing

        if (player.IsHoldingItem())
        {
            GameObject held = player.GetHeldItem();
            if (held.TryGetComponent<ItemData>(out var data))
            {
                bool accepted = false;

                if (data.itemType == ItemType.Flour && !hasFlour)
                {
                    hasFlour = true;
                    accepted = true;
                }
                else if (data.itemType == ItemType.Sugar && !hasSugar)
                {
                    hasSugar = true;
                    accepted = true;
                }
                else if (data.itemType == ItemType.Water && !hasWater)
                {
                    hasWater = true;
                    accepted = true;
                }

                if (accepted)
                {
                    Debug.Log($"[DOUGHMAKER] Accepted {data.itemType}. Consuming item.");
                    Destroy(player.RemoveHeldItem());
                    UpdateUI();
                    CheckIngredients();
                }
                else
                {
                    Debug.Log($"[DOUGHMAKER] Cannot accept {data.itemType}. Already have it or it's not a valid ingredient.");
                }
            }
            else
            {
                Debug.Log($"[DOUGHMAKER] Item {held.name} has no ItemData component!");
            }
        }
        else
        {
            Debug.Log("[DOUGHMAKER] Hand is empty. Use with Flour, Sugar, or Water.");
        }
    }

    private void CheckIngredients()
    {
        if (hasFlour && hasSugar && hasWater)
        {
            StartMixing();
        }
    }

    private void StartMixing()
    {
        isMixing = true;
        mixingTimer = 0f;
        
        if (timerCanvas != null) timerCanvas.SetActive(true);
        if (timerFillImage != null) timerFillImage.fillAmount = 0f;

        Debug.Log("[DOUGHMAKER] Mixing started...");
    }

    private void FinishMixing()
    {
        isMixing = false;
        hasFlour = false;
        hasSugar = false;
        hasWater = false;
        
        if (timerCanvas != null) timerCanvas.SetActive(false);
        
        UpdateUI();
        ProduceDough();
        
        Debug.Log("[DOUGHMAKER] Mixing finished. Dough produced in bin.");
    }

    private void ProduceDough()
    {
        if (doughBin != null)
        {
            doughBin.AddDough();
        }
    }

    private void UpdateUI()
    {
        if (flourIndicator != null) flourIndicator.SetActive(hasFlour);
        if (sugarIndicator != null) sugarIndicator.SetActive(hasSugar);
        if (waterIndicator != null) waterIndicator.SetActive(hasWater);
    }

    public string GetInteractText(PlayerController player)
    {
        if (isMixing)
        {
            float remaining = mixingTime - mixingTimer;
            return $"Mixing… {remaining:F1}s";
        }

        string needed = "";
        if (!hasFlour) needed += " Flour";
        if (!hasSugar) needed += " Sugar";
        if (!hasWater) needed += " Water";
        
        if (needed == "") return "Ready to Mix!";
        return "Needs:" + needed;
    }
}
