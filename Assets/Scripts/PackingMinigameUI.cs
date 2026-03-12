using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PackingMinigameUI : MonoBehaviour
{
    public static PackingMinigameUI Instance;

    [Header("UI Panels")]
    public GameObject windowRoot;
    public Transform trayGrid;      // Where pandesals are spawned
    public GameObject pandesalUIPrefab; // The draggable pandesal
    public RectTransform bagDropZone;
    
    [Header("UI Status")]
    public TextMeshProUGUI trayCountText;
    public TextMeshProUGUI bagCountText;
    [Header("Bulk Packing")]
    public Button packAllButton;
    public Button pack5Button;
    public Button pack10Button;
    public Button closeButton;

    private ItemData currentTrayData;
    private ItemData currentBagData;
    private PlayerController playerRef;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        windowRoot.SetActive(false);
    }

    private void Start()
    {
        closeButton.onClick.AddListener(CloseMinigame);
        
        if (packAllButton != null) packAllButton.onClick.AddListener(() => BulkPack(999));
        if (pack5Button != null) pack5Button.onClick.AddListener(() => BulkPack(5));
        if (pack10Button != null) pack10Button.onClick.AddListener(() => BulkPack(10));

        if (bagDropZone != null && !bagDropZone.CompareTag("PaperBagDropZone"))
        {
            bagDropZone.gameObject.tag = "PaperBagDropZone";
        }
    }

    public void OpenMinigame(PlayerController player, ItemData trayData, ItemData bagData)
    {
        playerRef = player;
        currentTrayData = trayData;
        currentBagData = bagData;

        // Freeze player and pause time
        player.enabled = false;
        Time.timeScale = 0f;
        
        if (GameManager.Instance != null)
            GameManager.Instance.ToggleHUD(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        windowRoot.SetActive(true);
        
        RefreshTrayVisuals();
        UpdateUI();
    }

    private void RefreshTrayVisuals()
    {
        ClearTray();
        SpawnPandesals();
        
        if (trayGrid != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(trayGrid.GetComponent<RectTransform>());
        }
    }

    private void ClearTray()
    {
        if (trayGrid == null) return;
        foreach (Transform child in trayGrid)
        {
            Destroy(child.gameObject);
        }
    }

    private void SpawnPandesals()
    {
        if (currentTrayData == null || trayGrid == null || pandesalUIPrefab == null) return;

        for (int i = 0; i < currentTrayData.count; i++)
        {
            Instantiate(pandesalUIPrefab, trayGrid);
        }
    }

    public void OnPandesalDropped()
    {
        if (currentTrayData != null && currentBagData != null)
        {
            currentTrayData.count--;
            currentBagData.count++;
            UpdateUI();
        }
    }

    private void BulkPack(int amount)
    {
        if (currentTrayData == null || currentBagData == null) return;

        int toMove = Mathf.Min(amount, currentTrayData.count);
        currentTrayData.count -= toMove;
        currentBagData.count += toMove;

        RefreshTrayVisuals();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentTrayData != null && trayCountText != null)
            trayCountText.text = "On Tray: " + currentTrayData.count;
        
        if (currentBagData != null && bagCountText != null)
            bagCountText.text = "In Bag: " + currentBagData.count;
    }

    public void CloseMinigame()
    {
        windowRoot.SetActive(false);

        // Resume player and time
        if (playerRef != null)
            playerRef.enabled = true;

        if (GameManager.Instance != null)
            GameManager.Instance.ToggleHUD(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // If tray is empty, destroy the physical tray gameobject
        if (currentTrayData != null && currentTrayData.count <= 0)
        {
            Debug.Log("[PACKING] Tray empty. Destroying tray object.");
            // Assuming the ItemData is on a child or the root of the tray object
            // Usually currentTrayData.gameObject is the thing to destroy
            Destroy(currentTrayData.gameObject);
        }
    }
}
