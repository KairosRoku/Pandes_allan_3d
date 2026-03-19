using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Oven — accepts TrayedShapedDough, bakes it over time, and can burn it.
///
/// State transitions destroy the current prefab and spawn the result prefab
/// in its place, consistent with the per-prefab-per-state pattern.
///
/// Timeline:
///   Insert → wait bakeTime → BakedPandesalTray
///   Wait burnTime total   → BurntPandesalTray
/// </summary>
public class Oven : MonoBehaviour, IInteractable, ISaveable
{
    [Header("Placement")]
    public Transform trayPoint;

    [Header("Timing")]
    public float baseBakeTime = 15f;
    public float baseBurnWindow = 3f;

    public float CurrentBakeTime
    {
        get
        {
            if (GameManager.Instance == null) return baseBakeTime;
            int lvl = GameManager.Instance.bakingUpgradeLevel;
            if (lvl >= 3) return 4f;
            if (lvl == 2) return 7f;
            if (lvl == 1) return 10f;
            return baseBakeTime;
        }
    }

    public float CurrentBurnTime
    {
        get
        {
            float window = baseBurnWindow;
            if (GameManager.Instance != null)
            {
                int lvl = GameManager.Instance.burnTimeUpgradeLevel;
                if (lvl >= 2) window = 10f;
                else if (lvl == 1) window = 5f;
            }
            return CurrentBakeTime + window;
        }
    }

    [Header("Result Prefabs")]
    [Tooltip("Prefab spawned when baking is complete.")]
    public GameObject bakedPandesalPrefab;

    [Tooltip("Prefab spawned when the tray is left in too long.")]
    public GameObject burntPandesalPrefab;

    [Header("UI")]
    public GameObject timerCanvas;
    public Image timerFillImage;
    public Color bakingColor = Color.green;
    public Color doneColor = Color.yellow;
    public Color burningColor = Color.red;

    // Runtime state
    private GameObject currentTray;
    private float      timer;
    private bool       isBaking;
    private bool       isDone;
    private bool       isBurnt;
    private Coroutine  breatheCoroutine;
    private Vector3    originalScale;
    private OvenVFX    ovenVFX;

    private void Awake()
    {
        originalScale = transform.localScale;
        ovenVFX = GetComponent<OvenVFX>();
    }

    private void Start()
    {
        if (timerCanvas != null)
            timerCanvas.SetActive(false);
    }

    // ---------------------------------------------------------------
    // IInteractable
    // ---------------------------------------------------------------

    public void Interact(PlayerController player)
    {
        if (player.IsHoldingItem())
        {
            if (currentTray != null) return; // Oven already occupied

            var held = player.GetHeldItem();
            if (held != null)
            {
                var data = held.GetComponentInChildren<ItemData>();
                if (data != null && data.itemType == ItemType.TrayedShapedDough)
                {
                    InsertTray(player);
                }
            }
        }
        else
        {
            // Only allow pickup once baking is done or burnt — not mid-bake
            if (currentTray != null && (isDone || isBurnt))
                RemoveTray(player);
        }
    }

    // ---------------------------------------------------------------
    // Baking timer
    // ---------------------------------------------------------------

    private void Update()
    {
        if (!isBaking)
        {
            if (timerCanvas != null && timerCanvas.activeSelf)
                timerCanvas.SetActive(false);
            return;
        }

        if (timerCanvas != null && !timerCanvas.activeSelf)
            timerCanvas.SetActive(true);

        timer += Time.deltaTime;

        UpdateUI();

        if (!isDone && timer >= CurrentBakeTime)
        {
            isDone = true;
            StopBreathe(); // Baking is done — oven goes quiet
            SwapTrayPrefab(bakedPandesalPrefab, "Baking complete — pick up the pandesal!");
        }

        if (!isBurnt && timer >= CurrentBurnTime)
        {
            isBurnt = true;
            SwapTrayPrefab(burntPandesalPrefab, "Tray burnt!");
        }
    }

    private void UpdateUI()
    {
        if (timerFillImage == null) return;

        float fill;
        Color color;

        if (!isDone)
        {
            // Baking phase: 0 to CurrentBakeTime
            fill = Mathf.Clamp01(timer / CurrentBakeTime);
            color = bakingColor;
        }
        else if (!isBurnt)
        {
            // Done phase: waiting to burn
            // We can show how close it is to burning by filling the rest (if CurrentBakeTime < CurrentBurnTime)
            // or just stay full and change color.
            float burnWindow = CurrentBurnTime - CurrentBakeTime;
            float burnProgress = (timer - CurrentBakeTime) / burnWindow;
            
            fill = 1f; // Keep it full
            color = Color.Lerp(doneColor, burningColor, burnProgress);
        }
        else
        {
            // Burnt phase
            fill = 1f;
            color = burningColor;
        }

        timerFillImage.fillAmount = fill;
        timerFillImage.color = color;
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    private void InsertTray(PlayerController player)
    {
        currentTray = player.RemoveHeldItem();
        SnapToTrayPoint(currentTray);

        timer    = 0f;
        isBaking = true;
        isDone   = false;
        isBurnt  = false;

        // Start breathing to show the oven is working
        if (breatheCoroutine != null) StopCoroutine(breatheCoroutine);
        breatheCoroutine = StartCoroutine(FlavorEffects.Breathe(transform, peakScale: 1.04f, period: 1.0f));
        if (ovenVFX != null) ovenVFX.StartVFX();
        if (SFXManager.Instance != null) SFXManager.Instance.StartOven();

        Debug.Log("[OVEN] Tray inserted. Baking started.");
    }

    private void RemoveTray(PlayerController player)
    {
        player.PickUpItem(currentTray);
        currentTray = null;
        isBaking    = false;
        isDone      = false;
        isBurnt     = false;

        StopBreathe();

        if (timerCanvas != null)
            timerCanvas.SetActive(false);

        Debug.Log("[OVEN] Tray removed by player.");
    }

    private void StopBreathe()
    {
        if (breatheCoroutine != null)
        {
            StopCoroutine(breatheCoroutine);
            breatheCoroutine = null;
        }
        transform.localScale = originalScale;
        if (ovenVFX != null) ovenVFX.StopVFX();
        if (SFXManager.Instance != null) SFXManager.Instance.StopOven();
    }

    /// <summary>
    /// Destroys the current tray object and spawns the result prefab at trayPoint.
    /// </summary>
    private void SwapTrayPrefab(GameObject prefab, string logLabel)
    {
        if (prefab == null)
        {
            Debug.LogWarning($"[OVEN] Result prefab not assigned! Assign it in the Inspector. ({logLabel})");
            return;
        }

        Destroy(currentTray);
        currentTray = Instantiate(prefab);
        SnapToTrayPoint(currentTray);

        Debug.Log($"[OVEN] {logLabel}");
    }

    private void SnapToTrayPoint(GameObject obj)
    {
        Counter.AutoFixBadPivots(obj);

        obj.transform.SetParent(trayPoint, true);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // Force physics bodies to stay perfectly still while in the oven
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
    }

    // ---------------------------------------------------------------
    // Prompt text
    // ---------------------------------------------------------------

    public string GetInteractText(PlayerController player)
    {
        if (currentTray == null) return "Insert Tray (E)";
        if (isBurnt)  return "Pick Up Burnt Pandesal (E)";
        if (isDone)   return "Pick Up Baked Pandesal (E)";

        float remaining = CurrentBakeTime - timer;
        return $"Baking… {remaining:F1}s remaining";
    }

    // ── ISaveable ───────────────────────────────────────────────────

    public StationSaveRecord CaptureState()
    {
        var record = new StationSaveRecord
        {
            scenePath = WorldStateSaver.GetScenePath(gameObject),
            itemType = ItemType.None,
            itemCount = 0,
            stockAmount = 0
        };

        if (currentTray != null)
        {
            var data = currentTray.GetComponentInChildren<ItemData>();
            if (data != null)
            {
                record.itemType = data.itemType;
                record.itemCount = data.count;
            }
        }
        return record;
    }

    public void RestoreState(StationSaveRecord record)
    {
        if (currentTray != null)
        {
            Destroy(currentTray);
            currentTray = null;
        }

        isBaking = false;
        isDone = false;
        isBurnt = false;
        timer = 0f;
        StopBreathe();
        if (timerCanvas != null) timerCanvas.SetActive(false);

        if (record.itemType != ItemType.None && WorldStateSaver.Instance != null)
        {
            GameObject prefab = WorldStateSaver.Instance.GetPrefab(record.itemType);
            if (prefab != null)
            {
                currentTray = Instantiate(prefab);
                SnapToTrayPoint(currentTray);

                var data = currentTray.GetComponentInChildren<ItemData>();
                if (data != null)
                {
                    data.itemType = record.itemType;
                    data.count = record.itemCount;
                }

                // Restore state based on item type
                if (record.itemType == ItemType.TrayedShapedDough)
                {
                    isBaking = true;
                    if (breatheCoroutine != null) StopCoroutine(breatheCoroutine);
                    breatheCoroutine = StartCoroutine(FlavorEffects.Breathe(transform, peakScale: 1.04f, period: 1.0f));
                    if (ovenVFX != null) ovenVFX.StartVFX();
                    if (SFXManager.Instance != null) SFXManager.Instance.StartOven();
                }
                else if (record.itemType == ItemType.BakedPandesalTray)
                {
                    isDone = true;
                }
                else if (record.itemType == ItemType.BurntPandesalTray)
                {
                    isBurnt = true;
                }
            }
        }
    }
}
