using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Static utility class for reusable "flavor" UI effects.
/// All methods return IEnumerators to be started via MonoBehaviour.StartCoroutine().
/// </summary>
public static class FlavorEffects
{
    // ─── Wiggle ──────────────────────────────────────────────────────────────
    // Quickly rotates the transform left-right a few times then snaps back.

    /// <summary>
    /// Wiggle a UI RectTransform (e.g. a counter text).
    /// Uses Quaternion composition to avoid Unity's 0–360 Euler wrapping drift.
    /// </summary>
    public static IEnumerator Wiggle(RectTransform rt, float duration = 0.35f, float angle = 12f, int bounces = 4)
    {
        if (rt == null) yield break;

        // Store as Quaternion — immune to the 0–360 Euler wrap issue
        Quaternion originalRot = rt.localRotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float decay = 1f - t;
            // Sine oscillation that decays to zero
            float z = Mathf.Sin(t * bounces * Mathf.PI * 2f) * angle * decay;
            rt.localRotation = originalRot * Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        // Snap back exactly to the original — no drift
        rt.localRotation = originalRot;
    }

    // ─── Wave ────────────────────────────────────────────────────────────────
    // Animates a TMP text so each character bobs up and down like a wave.

    /// <summary>
    /// Wave-animate every character in a TextMeshProUGUI for <duration> seconds.
    /// </summary>
    public static IEnumerator WaveText(TextMeshProUGUI label, float duration = 1.5f,
                                        float amplitude = 8f, float frequency = 2.5f)
    {
        if (label == null) yield break;

        label.ForceMeshUpdate();
        var textInfo = label.textInfo;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            label.ForceMeshUpdate();
            textInfo = label.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int matIdx = charInfo.materialReferenceIndex;
                int vertIdx = charInfo.vertexIndex;

                Vector3[] verts = textInfo.meshInfo[matIdx].vertices;
                float waveOffset = Mathf.Sin(elapsed * frequency * Mathf.PI * 2f - i * 0.5f) * amplitude;
                Vector3 offset = new Vector3(0f, waveOffset, 0f);

                verts[vertIdx + 0] += offset;
                verts[vertIdx + 1] += offset;
                verts[vertIdx + 2] += offset;
                verts[vertIdx + 3] += offset;
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                label.UpdateGeometry(meshInfo.mesh, i);
            }

            yield return null;
        }

        // Reset mesh to default positions
        label.ForceMeshUpdate();
    }

    // ─── Pulse Scale ─────────────────────────────────────────────────────────
    // Briefly enlarges then restores a transform's local scale.

    // ─── Breathe ─────────────────────────────────────────────────────────────
    // Continuously pulses a 3D Transform's scale (big → normal → big …)
    // while a machine is working. Stop it with StopCoroutine.

    /// <summary>
    /// Loops a gentle scale-breathe on <paramref name="target"/> until StopCoroutine is called.
    /// Designed for 3D GameObjects (DoughMaker, Oven) — uses localScale, not RectTransform.
    /// </summary>
    public static IEnumerator Breathe(Transform target, float peakScale = 1.04f, float period = 1.2f)
    {
        if (target == null) yield break;

        Vector3 original = target.localScale;
        Vector3 peak     = original * peakScale;

        while (true)
        {
            // Inhale: original → peak
            float elapsed = 0f;
            float half = period * 0.5f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / half);
                if (target == null) yield break;
                target.localScale = Vector3.Lerp(original, peak, t);
                yield return null;
            }

            // Exhale: peak → original
            elapsed = 0f;
            while (elapsed < half)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / half);
                if (target == null) yield break;
                target.localScale = Vector3.Lerp(peak, original, t);
                yield return null;
            }
        }
    }

    // ─── Pulse Scale ─────────────────────────────────────────────────────────
    // Briefly enlarges then restores a transform's local scale.


    /// <summary>
    /// Pop-scale a transform up then back to original (like a punch tween).
    /// </summary>
    public static IEnumerator PulseScale(Transform target, float peakScale = 1.25f, float duration = 0.25f)
    {
        if (target == null) yield break;

        Vector3 original = target.localScale;
        Vector3 peak = original * peakScale;

        float half = duration * 0.5f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / half;
            target.localScale = Vector3.Lerp(original, peak, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        elapsed = 0f;
        // Scale back down
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / half;
            target.localScale = Vector3.Lerp(peak, original, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        target.localScale = original;
    }

    // ─── Bounce Scale ────────────────────────────────────────────────────────
    // Grows to a peak, overshoots slightly small, then settles — like a rubber band.

    /// <summary>
    /// Bounce-scale a RectTransform: pop up → tiny overshoot down → settle at original.
    /// Great replacement for wiggle on counter texts; no rotation is touched.
    /// </summary>
    public static IEnumerator BounceScale(RectTransform rt, float peakScale = 1.2f, float duration = 0.3f)
    {
        if (rt == null) yield break;

        Vector3 original = rt.localScale;

        // Phase timings (fraction of total duration)
        float upFraction   = 0.35f; // grow up
        float downFraction = 0.30f; // overshoot below original
        float backFraction = 0.35f; // settle back

        Vector3 peak      = original * peakScale;
        Vector3 undershoot = original * 0.92f;

        float elapsed = 0f;
        float phase;

        // 1) Grow up to peak
        phase = duration * upFraction;
        elapsed = 0f;
        while (elapsed < phase)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / phase);
            rt.localScale = Vector3.LerpUnclamped(original, peak, t);
            yield return null;
        }

        // 2) Spring back past original (small undershoot)
        phase = duration * downFraction;
        elapsed = 0f;
        while (elapsed < phase)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / phase);
            rt.localScale = Vector3.LerpUnclamped(peak, undershoot, t);
            yield return null;
        }

        // 3) Settle precisely at original
        phase = duration * backFraction;
        elapsed = 0f;
        while (elapsed < phase)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / phase);
            rt.localScale = Vector3.LerpUnclamped(undershoot, original, t);
            yield return null;
        }

        rt.localScale = original;
    }

    // ─── Hover Scale (used by PointerEnter/Exit events) ─────────────────────

    /// <summary>
    /// Smoothly scale a RectTransform to a target scale (use for hover in/out).
    /// </summary>
    public static IEnumerator SmoothScale(RectTransform rt, Vector3 targetScale, float duration = 0.12f)
    {
        if (rt == null) yield break;

        Vector3 startScale = rt.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            rt.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        rt.localScale = targetScale;
    }
}
