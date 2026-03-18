using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DoughMaker3000 : MonoBehaviour, IInteractable
{
    public DoughBin doughBin;
    public GameObject doughItemPrefab;

    [Header("UI Indicators (Checked)")]
    [UnityEngine.Serialization.FormerlySerializedAs("flourIndicator")]
    public GameObject flourIndicatorChecked;
    [UnityEngine.Serialization.FormerlySerializedAs("sugarIndicator")]
    public GameObject sugarIndicatorChecked;
    [UnityEngine.Serialization.FormerlySerializedAs("waterIndicator")]
    public GameObject waterIndicatorChecked;

    [Header("UI Indicators (Unchecked)")]
    public GameObject flourIndicatorUnchecked;
    public GameObject sugarIndicatorUnchecked;
    public GameObject waterIndicatorUnchecked;

    [Header("Mixing Timing")]
    public float baseMixingTime = 15f;
    private float mixingTimer;
    private bool isMixing;

    public float CurrentMixingTime
    {
        get
        {
            if (GameManager.Instance == null) return baseMixingTime;
            int lvl = GameManager.Instance.doughMakingUpgradeLevel;
            if (lvl >= 3) return 4f;
            if (lvl == 2) return 7f;
            if (lvl == 1) return 10f;
            return baseMixingTime;
        }
    }

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
                timerFillImage.fillAmount = Mathf.Clamp01(mixingTimer / CurrentMixingTime);

            if (mixingTimer >= CurrentMixingTime)
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
        if (flourIndicatorChecked != null) flourIndicatorChecked.SetActive(hasFlour);
        if (sugarIndicatorChecked != null) sugarIndicatorChecked.SetActive(hasSugar);
        if (waterIndicatorChecked != null) waterIndicatorChecked.SetActive(hasWater);

        if (flourIndicatorUnchecked != null) flourIndicatorUnchecked.SetActive(!hasFlour);
        if (sugarIndicatorUnchecked != null) sugarIndicatorUnchecked.SetActive(!hasSugar);
        if (waterIndicatorUnchecked != null) waterIndicatorUnchecked.SetActive(!hasWater);
    }

    public string GetInteractText(PlayerController player)
    {
        if (isMixing)
        {
            float remaining = CurrentMixingTime - mixingTimer;
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
