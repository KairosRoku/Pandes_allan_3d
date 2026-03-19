using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach to any UI element (Button, Image, etc.) to give it a smooth scale-up
/// on hover and scale-back on exit. Works with Time.timeScale = 0.
/// </summary>
public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("How much bigger the element grows on hover (1.1 = 10% larger)")]
    public float hoverScale = 1.1f;

    [Tooltip("How quickly it scales in/out (seconds)")]
    public float duration = 0.12f;

    private RectTransform rt;
    private Vector3 originalScale;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (rt == null) rt = gameObject.AddComponent<RectTransform>();
        originalScale = rt.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(FlavorEffects.SmoothScale(rt, originalScale * hoverScale, duration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(FlavorEffects.SmoothScale(rt, originalScale, duration));
    }

    // Ensure we reset if disabled mid-animation
    private void OnDisable()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        if (rt != null) rt.localScale = originalScale;
    }
}
