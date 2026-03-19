using UnityEngine;

/// <summary>
/// Gold coin-burst VFX spawned when the player earns money.
/// Attach to any GameObject in the scene (e.g., near the CustomerWindow).
/// GameManager.AddMoney() calls PlayBurst() automatically.
/// </summary>
public class MoneyVFX : MonoBehaviour
{
    [Tooltip("How many coin particles to burst per sale.")]
    public int burstCount = 20;

    [Tooltip("Optional world-space position override; if null, uses this GameObject's position.")]
    public Transform spawnPoint;

    private ParticleSystem coinPS;

    private void Awake()
    {
        BuildCoins();
    }

    private void BuildCoins()
    {
        GameObject go = new GameObject("_MoneyVFX");
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        coinPS = go.AddComponent<ParticleSystem>();

        // ── Main ──────────────────────────────────────────────────────
        var main = coinPS.main;
        main.loop            = false;        // One-shot burst
        main.playOnAwake     = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.6f, 1.1f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1.2f, 3.0f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.startRotation   = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(1f,  0.85f, 0.1f, 1f),   // bright gold
            new Color(0.9f, 0.65f, 0f,  1f));   // deep gold
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 60;
        main.gravityModifier = 0.6f;            // coins arc then fall

        // ── Emission: burst only ──────────────────────────────────────
        var emission = coinPS.emission;
        emission.enabled      = true;
        emission.rateOverTime = 0f;             // No continuous emission
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)burstCount)
        });

        // ── Shape: sphere → coins spray in all directions ─────────────
        var shape = coinPS.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius    = 0.05f;

        // ── Color over lifetime: bright → fade ────────────────────────
        var col = coinPS.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f,  0.95f, 0.3f), 0f),
                    new GradientColorKey(new Color(1f,  0.7f,  0.0f), 0.5f),
                    new GradientColorKey(new Color(0.8f, 0.5f, 0.0f), 1f) },
            new[] { new GradientAlphaKey(1f,  0f),
                    new GradientAlphaKey(1f,  0.6f),
                    new GradientAlphaKey(0f,  1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // ── Size over lifetime: shrink slightly as they fall ──────────
        var size = coinPS.sizeOverLifetime;
        size.enabled = true;
        var sc = new AnimationCurve();
        sc.AddKey(0f,   1f);
        sc.AddKey(0.5f, 1.2f);  // brief flash bigger
        sc.AddKey(1f,   0.3f);
        size.size = new ParticleSystem.MinMaxCurve(1f, sc);

        // ── Rotation over lifetime: spin the coins ────────────────────
        var rot = coinPS.rotationOverLifetime;
        rot.enabled = true;
        rot.z       = new ParticleSystem.MinMaxCurve(-360f * Mathf.Deg2Rad, 360f * Mathf.Deg2Rad);
    }

    // ── Public API ────────────────────────────────────────────────────

    /// <summary>
    /// Fire a coin burst at this VFX object's position (or spawnPoint if assigned).
    /// </summary>
    public void PlayBurst()
    {
        if (coinPS == null) return;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        coinPS.transform.position = pos;

        coinPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        coinPS.Play();
    }
}
