using UnityEngine;
using TMPro;

public enum TutorialStep
{
    NotStarted,
    MakeDough,          // Needs to supply the DoughMaker3000
    CollectDough,       // Needs to grab Dough from DoughBin
    KneadDough,         // Needs to knead dough (hold DoughKnead or place it on table)
    ShapeDough,         // Needs to shape dough (hold ShapedDough or place)
    GetTray,            // Needs to get a tray and combine with ShapedDough
    Bake,               // Needs to put tray in oven and wait for BakedPandesalTray
    Pack,               // Needs to get a paper bag and pack pandesal
    Serve,              // Needs to serve customer
    Completed
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI References")]
    public GameObject tutorialPanel; // A thin panel at the top/corner of the screen
    public TextMeshProUGUI tutorialText;

    [Header("Navigation Guide")]
    [Tooltip("Assign a LineRenderer component. This will draw a guide line from player to target!")]
    public LineRenderer pathLine;

    private TutorialStep currentStep = TutorialStep.NotStarted;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    private void Start()
    {
        // Give GameManager time to load save data, then check if it's Day 1
        Invoke("CheckStartTutorial", 1f);
    }
    
    private void CheckStartTutorial()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentDay == 1)
        {
            // Start tutorial
            currentStep = TutorialStep.MakeDough;
            if (tutorialPanel != null) tutorialPanel.SetActive(true);
            UpdateTutorialUI();
        }
        else
        {
            currentStep = TutorialStep.Completed;
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (currentStep == TutorialStep.Completed || currentStep == TutorialStep.NotStarted) return;
        
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;
        
        UpdateGuideLine(player);
        
        switch (currentStep)
        {
            case TutorialStep.MakeDough:
                DoughBin doughBin = FindObjectOfType<DoughBin>();
                if (doughBin != null && doughBin.doughCount > 0)
                {
                    AdvanceStep(TutorialStep.CollectDough);
                }
                break;
                
            case TutorialStep.CollectDough:
                if (player.IsHoldingItem())
                {
                    var data = player.GetHeldItem().GetComponentInChildren<ItemData>();
                    if (data != null && data.itemType == ItemType.Dough)
                    {
                        AdvanceStep(TutorialStep.KneadDough);
                    }
                }
                break;
                
            case TutorialStep.KneadDough:
                if (HasItemInWorld(ItemType.DoughKnead) || (player.IsHoldingItem() && player.GetHeldItem().GetComponentInChildren<ItemData>()?.itemType == ItemType.DoughKnead))
                {
                    AdvanceStep(TutorialStep.ShapeDough);
                }
                break;
                
            case TutorialStep.ShapeDough:
                if (HasItemInWorld(ItemType.ShapedDough) || (player.IsHoldingItem() && player.GetHeldItem().GetComponentInChildren<ItemData>()?.itemType == ItemType.ShapedDough))
                {
                    AdvanceStep(TutorialStep.GetTray);
                }
                break;
                
            case TutorialStep.GetTray:
                if (HasItemInWorld(ItemType.TrayedShapedDough) || (player.IsHoldingItem() && player.GetHeldItem().GetComponentInChildren<ItemData>()?.itemType == ItemType.TrayedShapedDough))
                {
                    AdvanceStep(TutorialStep.Bake);
                }
                break;
                
            case TutorialStep.Bake:
                if (HasItemInWorld(ItemType.BakedPandesalTray) || (player.IsHoldingItem() && player.GetHeldItem().GetComponentInChildren<ItemData>()?.itemType == ItemType.BakedPandesalTray))
                {
                    AdvanceStep(TutorialStep.Pack);
                    
                    // Force the very first customer to spawn early so the player doesn't have to wait!
                    CustomerWindow window = FindObjectOfType<CustomerWindow>();
                    if (window != null && window.customerQueue.Count == 0)
                    {
                        window.TrySpawnCustomer(force: true);
                    }
                }
                break;
                
            case TutorialStep.Pack:
                if (player.IsHoldingItem())
                {
                    var data = player.GetHeldItem().GetComponentInChildren<ItemData>();
                    // Needs to be a paper bag with at least 1 pandesal inside
                    if (data != null && data.itemType == ItemType.PaperBag && data.count > 0)
                    {
                        AdvanceStep(TutorialStep.Serve);
                    }
                }
                break;
                
            case TutorialStep.Serve:
                // This step is advanced externally by the CustomerWindow calling CompleteTutorial()
                break;
        }
    }
    
    private bool HasItemInWorld(ItemType type)
    {
        return FindItemInWorldTransform(type) != null;
    }
    
    private Transform FindItemInWorldTransform(ItemType type)
    {
        // Check all tables to see if the required item exists there
        Counter[] counters = FindObjectsOfType<Counter>();
        foreach (var c in counters)
        {
            var data = c.GetComponentInChildren<ItemData>();
            if (data != null && data.itemType == type) return c.transform;
        }
        return null;
    }

    private void UpdateGuideLine(PlayerController player)
    {
        if (pathLine == null) return;

        Transform target = GetCurrentTarget(player);
        if (target != null)
        {
            pathLine.enabled = true;
            // Draw line from player's waist to target's center
            pathLine.SetPosition(0, player.transform.position + Vector3.up * 0.5f);
            pathLine.SetPosition(1, target.position + Vector3.up * 0.5f);
        }
        else
        {
            pathLine.enabled = false;
        }
    }

    private Transform GetCurrentTarget(PlayerController player)
    {
        switch (currentStep)
        {
            case TutorialStep.MakeDough:
                DoughMaker3000 dm = FindObjectOfType<DoughMaker3000>();
                if (dm != null)
                {
                    // Point straight to the DoughMaker if we're carrying something or it's currently running
                    if (dm.IsMixing || player.IsHoldingItem()) 
                        return dm.transform;

                    // Which ingredient are we missing specifically?
                    ItemType missing = ItemType.None;
                    if (!dm.HasFlour) missing = ItemType.Flour;
                    else if (!dm.HasSugar) missing = ItemType.Sugar;
                    else if (!dm.HasWater) missing = ItemType.Water;

                    if (missing != ItemType.None)
                    {
                        var dispensers = FindObjectsOfType<Dispenser>();
                        foreach (var d in dispensers) if (d.itemType == missing) return d.transform;

                        var racks = FindObjectsOfType<IngredientRack>();
                        foreach (var r in racks) if (r.itemType == missing) return r.transform;
                    }
                }
                return dm?.transform;
                
            case TutorialStep.CollectDough:
                return FindObjectOfType<DoughBin>()?.transform;
                
            case TutorialStep.KneadDough:
            case TutorialStep.ShapeDough:
                if (!player.IsHoldingItem() && HasItemInWorld(ItemType.Dough)) return FindItemInWorldTransform(ItemType.Dough);
                // Point to an empty processing table or one that already has dough on it
                return FindObjectOfType<ProcessingTable>()?.transform;
                
            case TutorialStep.GetTray:
                if (!player.IsHoldingItem() || player.GetHeldItem().GetComponentInChildren<ItemData>()?.itemType != ItemType.Tray) 
                    return FindObjectOfType<TrayBin>()?.transform;
                return FindItemInWorldTransform(ItemType.ShapedDough);

            case TutorialStep.Bake:
                if (!player.IsHoldingItem()) return FindItemInWorldTransform(ItemType.TrayedShapedDough);
                return FindObjectOfType<Oven>()?.transform;
                
            case TutorialStep.Pack:
                if (!player.IsHoldingItem() || player.GetHeldItem().GetComponentInChildren<ItemData>()?.itemType != ItemType.PaperBag) 
                    return FindObjectOfType<PaperBagBin>()?.transform;
                return FindItemInWorldTransform(ItemType.BakedPandesalTray);
                
            case TutorialStep.Serve:
                return FindObjectOfType<CustomerWindow>()?.transform;
        }
        return null;
    }

    private void AdvanceStep(TutorialStep nextStep)
    {
        currentStep = nextStep;
        UpdateTutorialUI();
        
        // Wiggle to catch their attention
        if (tutorialText != null)
        {
            var rt = tutorialText.GetComponent<RectTransform>();
            if (rt != null) StartCoroutine(FlavorEffects.Wiggle(rt));
        }

        if (SFXManager.Instance != null) SFXManager.Instance.PlayButtonPress();
    }
    
    public void CompleteTutorial()
    {
        if (currentStep == TutorialStep.Serve)
        {
            currentStep = TutorialStep.Completed;
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            if (pathLine != null) pathLine.enabled = false;
            Debug.Log("[TUTORIAL] Tutorial Completed!");
        }
    }
    
    private void UpdateTutorialUI()
    {
        if (tutorialText == null) return;
        
        switch (currentStep)
        {
            case TutorialStep.MakeDough:
                tutorialText.text = "Step 1: Grab Flour, Sugar, and Water. Place them in the DoughMaker3000 to mix.";
                break;
            case TutorialStep.CollectDough:
                tutorialText.text = "Step 2: Grab the fresh Dough from the Dough Bin.";
                break;
            case TutorialStep.KneadDough:
                tutorialText.text = "Step 3: Place the Dough on a Processing Table and Knead it [E].";
                break;
            case TutorialStep.ShapeDough:
                tutorialText.text = "Step 4: Click the Kneaded Dough on the table to Shape it [E].";
                break;
            case TutorialStep.GetTray:
                tutorialText.text = "Step 5: Pick up a Tray from the Tray Bin. Combine it with the Shaped Dough.";
                break;
            case TutorialStep.Bake:
                tutorialText.text = "Step 6: Place the full Tray inside the Oven. Wait for it to bake.";
                break;
            case TutorialStep.Pack:
                tutorialText.text = "Step 7: Pick up an empty Paper Bag. Interact with the Baked Tray to pack Pandesal inside.";
                break;
            case TutorialStep.Serve:
                tutorialText.text = "Step 8: Bring your packed Paper Bag to the Window to serve the Customer!";
                break;
        }
    }
}
