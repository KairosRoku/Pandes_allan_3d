using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggablePandesal : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    private Transform startParent;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
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
