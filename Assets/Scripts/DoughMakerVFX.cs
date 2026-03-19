using UnityEngine;

/// <summary>
/// Procedurally-built steam VFX for the DoughMaker3000.
/// Attach to the DoughMaker3000 GameObject.
/// DoughMaker3000 calls StartVFX() / StopVFX() automatically.
/// </summary>
public class DoughMakerVFX : MonoBehaviour
{
    [Tooltip("Local position above the machine where steam rises from.")]
    public Vector3 emitOffset = new Vector3(0f, 0.6f, 0f);

    [Tooltip("Particles per second while mixing.")]
    public float emissionRate = 14f;

    private ParticleSystem steamPS;

    private void Awake()
    {
        GameObject vfxGO = new GameObject("_SteamVFX");
        vfxGO.transform.SetParent(transform);
        vfxGO.transform.localPosition = emitOffset;
        vfxGO.transform.localRotation = Quaternion.identity;

        steamPS = vfxGO.AddComponent<ParticleSystem>();
        BuildSteam();

        // Start stopped
        var emit = steamPS.emission;
        emit.enabled = false;
        steamPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void BuildSteam()
    {
        // ── Main ──────────────────────────────────────────────────────
        var main = steamPS.main;
        main.loop              = true;
        main.playOnAwake       = false;
        main.startLifetime     = new ParticleSystem.MinMaxCurve(1.0f, 1.8f);
        main.startSpeed        = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        main.startSize         = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startRotation     = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startColor        = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 0.75f),
            new Color(0.82f, 0.93f, 1f,  0.5f));
        main.simulationSpace   = ParticleSystemSimulationSpace.World;
        main.maxParticles      = 60;
        main.gravityModifier   = -0.04f;  // gently float upward

        // ── Emission ─────────────────────────────────────────────────
        var emission = steamPS.emission;
        emission.enabled      = true;
        emission.rateOverTime = emissionRate;

        // ── Shape: wide-ish upward cone ───────────────────────────────
        var shape = steamPS.shape;
        shape.enabled    = true;
        shape.shapeType  = ParticleSystemShapeType.Cone;
        shape.angle      = 22f;
        shape.radius     = 0.07f;

        // ── Color over lifetime: fade to transparent ──────────────────
        var col = steamPS.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.8f, 0.8f, 0.8f), 1f) },
            new[] { new GradientAlphaKey(0.75f, 0f),
                    new GradientAlphaKey(0.2f,  0.7f),
                    new GradientAlphaKey(0f,    1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // ── Size over lifetime: grow as it rises ──────────────────────
        var size = steamPS.sizeOverLifetime;
        size.enabled = true;
        var sc = new AnimationCurve();
        sc.AddKey(0f, 0.3f);
        sc.AddKey(0.35f, 1f);
        sc.AddKey(1f,  1.6f);
        size.size = new ParticleSystem.MinMaxCurve(1f, sc);

        // ── Velocity over lifetime: random sway ───────────────────────
        var vel = steamPS.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        vel.y       = new ParticleSystem.MinMaxCurve(0.15f,  0.4f);
        vel.z       = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
    }

    // ── Public API ────────────────────────────────────────────────────

    public void StartVFX()
    {
        if (steamPS == null) return;
        var emit = steamPS.emission;
        emit.enabled = true;
        if (!steamPS.isPlaying) steamPS.Play();
    }

    public void StopVFX()
    {
        if (steamPS == null) return;
        // Stop new particles; let existing ones die naturally
        steamPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}
