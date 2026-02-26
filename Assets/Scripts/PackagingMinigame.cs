using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PackagingMinigame : MonoBehaviour
{
    public MinigameManager manager;
    public RectTransform paperBagArea;
    public GameObject pandesalUIPrefab;
    public Transform trayContainer;
    
    private int successCount = 0;
    private int totalPandesals = 12; // Example amount per tray
    private int processedCount = 0;

    public void OnEnable()
    {
        successCount = 0;
        processedCount = 0;
        // Spawn 12 pandesals on the tray
        foreach (Transform child in trayContainer) Destroy(child.gameObject);
        for (int i = 0; i < totalPandesals; i++)
        {
            Instantiate(pandesalUIPrefab, trayContainer);
        }
    }

    public void PandesalDropped(bool inBag)
    {
        processedCount++;
        if (inBag) successCount++;

        if (processedCount >= totalPandesals)
        {
            manager.FinishPackaging(successCount);
        }
    }
}

public class DraggablePandesal : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPos;
    private PackagingMinigame minigame;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        minigame = GetComponentInParent<PackagingMinigame>();
        startPos = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if over bag
        if (RectTransformUtility.RectangleContainsScreenPoint(minigame.paperBagArea, Input.mousePosition))
        {
            minigame.PandesalDropped(true);
            Destroy(gameObject);
        }
        else
        {
            // Missed!
            minigame.PandesalDropped(false);
            Destroy(gameObject);
        }
    }
}
