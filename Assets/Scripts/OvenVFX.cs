using UnityEngine;

/// <summary>
/// Procedurally-built smoke + ember VFX for the Oven.
/// Attach to the Oven GameObject.
/// Oven.cs calls StartVFX() / StopVFX() automatically.
/// </summary>
public class OvenVFX : MonoBehaviour
{
    [Tooltip("Local position above the oven door/top where smoke rises.")]
    public Vector3 smokeOffset = new Vector3(0f, 0.7f, 0f);

    [Tooltip("Local position where embers/sparks float out.")]
    public Vector3 emberOffset = new Vector3(0f, 0.3f, 0.2f);

    private ParticleSystem smokePS;
    private ParticleSystem emberPS;

    private void Awake()
    {
        BuildSmoke();
        BuildEmbers();

        StopVFX();
    }

    // ── Build smoke system ────────────────────────────────────────────

    private void BuildSmoke()
    {
        GameObject go = new GameObject("_SmokeVFX");
        go.transform.SetParent(transform);
        go.transform.localPosition = smokeOffset;
        go.transform.localRotation = Quaternion.identity;

        smokePS = go.AddComponent<ParticleSystem>();

        // Main
        var main = smokePS.main;
        main.loop            = true;
        main.playOnAwake     = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(2.0f, 3.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.2f, 0.55f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        main.startRotation   = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(0.55f, 0.55f, 0.55f, 0.7f),
            new Color(0.3f,  0.3f,  0.3f,  0.4f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 80;
        main.gravityModifier = -0.03f;

        // Emission
        var emission = smokePS.emission;
        emission.enabled      = true;
        emission.rateOverTime = 10f;

        // Shape
        var shape = smokePS.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle     = 18f;
        shape.radius    = 0.1f;

        // Color over lifetime: dark grey → transparent
        var col = smokePS.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.5f, 0.5f, 0.5f), 0f),
                    new GradientColorKey(new Color(0.25f, 0.25f, 0.25f), 1f) },
            new[] { new GradientAlphaKey(0.7f, 0f),
                    new GradientAlphaKey(0.3f, 0.6f),
                    new GradientAlphaKey(0f,   1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // Size over lifetime: billow outward
        var size = smokePS.sizeOverLifetime;
        size.enabled = true;
        var sc = new AnimationCurve();
        sc.AddKey(0f,   0.4f);
        sc.AddKey(0.4f, 1f);
        sc.AddKey(1f,   1.8f);
        size.size = new ParticleSystem.MinMaxCurve(1f, sc);

        // Velocity: gentle sway
        var vel = smokePS.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;
        vel.x       = new ParticleSystem.MinMaxCurve(-0.07f, 0.07f);
        vel.y       = new ParticleSystem.MinMaxCurve(0.15f,  0.35f);
        vel.z       = new ParticleSystem.MinMaxCurve(-0.07f, 0.07f);
    }

    // ── Build ember system ────────────────────────────────────────────

    private void BuildEmbers()
    {
        GameObject go = new GameObject("_EmberVFX");
        go.transform.SetParent(transform);
        go.transform.localPosition = emberOffset;
        go.transform.localRotation = Quaternion.identity;

        emberPS = go.AddComponent<ParticleSystem>();

        // Main
        var main = emberPS.main;
        main.loop            = true;
        main.playOnAwake     = false;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.015f, 0.045f);
        main.startColor      = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.5f, 0.05f, 1f),
            new Color(1f, 0.8f, 0.1f,  1f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles    = 40;
        main.gravityModifier = 0.08f;

        // Emission — sparse sparks
        var emission = emberPS.emission;
        emission.enabled      = true;
        emission.rateOverTime = 5f;

        // Shape: hemisphere — sparks scatter outward and up
        var shape = emberPS.shape;
        shape.enabled   = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius    = 0.12f;

        // Color over lifetime: orange → red → transparent
        var col = emberPS.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.6f, 0.05f), 0f),
                    new GradientColorKey(new Color(0.8f, 0.1f, 0f),   1f) },
            new[] { new GradientAlphaKey(1f,  0f),
                    new GradientAlphaKey(0.6f, 0.5f),
                    new GradientAlphaKey(0f,   1f) });
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // Size over lifetime: shrink as it dies
        var size = emberPS.sizeOverLifetime;
        size.enabled = true;
        var sc = new AnimationCurve();
        sc.AddKey(0f, 1f);
        sc.AddKey(1f, 0.1f);
        size.size = new ParticleSystem.MinMaxCurve(1f, sc);
    }

    // ── Public API ────────────────────────────────────────────────────

    public void StartVFX()
    {
        SetVFX(true);
    }

    public void StopVFX()
    {
        SetVFX(false);
    }

    private void SetVFX(bool play)
    {
        if (smokePS != null)
        {
            if (play) smokePS.Play();
            else      smokePS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (emberPS != null)
        {
            if (play) emberPS.Play();
            else      emberPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
