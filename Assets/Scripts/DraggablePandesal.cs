using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePandesal : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Scale")]
    [Tooltip("Scale factor on hover (1.15 = 15% bigger)")]
    public float hoverScale = 1.15f;
    public float hoverDuration = 0.12f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Transform startParent;
    private Canvas canvas;
    private Vector3 originalScale;
    private Coroutine hoverCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        originalScale = rectTransform.localScale;
    }

    // ─── Hover ───────────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(FlavorEffects.SmoothScale(rectTransform, originalScale * hoverScale, hoverDuration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(FlavorEffects.SmoothScale(rectTransform, originalScale, hoverDuration));
    }

    // ─── Drag ────────────────────────────────────────────────────────────────

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Cancel hover and reset scale while dragging
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        rectTransform.localScale = originalScale;

        startPosition = rectTransform.anchoredPosition;
        startParent = transform.parent;
        
        // Move to top of hierarchy so it's not hidden by other items
        transform.SetParent(canvas.transform);
        
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Follow mouse
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        // Check if we dropped on the bag
        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag("PaperBagDropZone"))
        {
            // Bounce the paper bag drop zone
            var bagRt = eventData.pointerEnter.GetComponent<RectTransform>();
            if (bagRt != null)
                StartCoroutine(FlavorEffects.BounceScale(bagRt, peakScale: 1.15f, duration: 0.25f));

            PackingMinigameUI.Instance.OnPandesalDropped();
            Destroy(gameObject);
        }
        else
        {
            // Reset to tray
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPosition;
        }
    }
}
