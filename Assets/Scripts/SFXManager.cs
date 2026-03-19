using UnityEngine;

/// <summary>
/// Centralized SFX Manager for easy access all over the project.
/// </summary>
public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Audio Sources (Requires Assigment)")]
    [Tooltip("For short pop/click sounds")]
    public AudioSource oneShotSource;
    [Tooltip("For walking and dashing")]
    public AudioSource footstepSource;
    [Tooltip("For looping oven baking")]
    public AudioSource ovenSource;
    [Tooltip("For looping dough maker mixing")]
    public AudioSource doughMakerSource;

    [Header("UI & Interactions")]
    public AudioClip buttonPressSFX;
    public AudioClip itemGrabSFX;
    public AudioClip itemPutOnTableSFX;
    public AudioClip itemOnDoughMakerSFX;
    public AudioClip pandesalPlaceOnPaperBagSFX;
    
    [Header("Minigames")]
    public AudioClip kneadingSFX;
    public AudioClip rollingSFX;

    [Header("Machines (Looping)")]
    public AudioClip ovenLoopingSFX;
    public AudioClip doughMakerLoopingSFX;

    [Header("Economy & Events")]
    public AudioClip customerInSFX;
    public AudioClip customerPaidSFX;
    public AudioClip buySFX;
    public AudioClip dayEndSFX;

    [Header("Player")]
    public AudioClip walkSFX;
    public AudioClip dashSFX;

    private float rollSfxTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── UI / Simple Interactions ──────────────────────────────
    public void PlayButtonPress() { PlayOneShotSafe(buttonPressSFX); }
    public void PlayItemGrab() { PlayOneShotSafe(itemGrabSFX); }
    public void PlayItemPutOnTable() { PlayOneShotSafe(itemPutOnTableSFX); }
    public void PlayItemOnDoughMaker() { PlayOneShotSafe(itemOnDoughMakerSFX); }
    public void PlayPandesalPlace() { PlayOneShotSafe(pandesalPlaceOnPaperBagSFX); }
    
    // ─── Events ────────────────────────────────────────────────
    public void PlayCustomerIn() { PlayOneShotSafe(customerInSFX); }
    public void PlayCustomerPaid() { PlayOneShotSafe(customerPaidSFX); }
    public void PlayBuy() { PlayOneShotSafe(buySFX); }
    public void PlayDayEnd() { PlayOneShotSafe(dayEndSFX); }

    // ─── Minigames ─────────────────────────────────────────────
    public void PlayKneading() { PlayOneShotSafe(kneadingSFX); }

    public void PlayRolling() 
    { 
        if (rollingSFX == null || oneShotSource == null) return;
        // Throttle the rolling sound so it doesn't machine-gun if updated every frame
        if (Time.time > rollSfxTimer)
        {
            oneShotSource.PlayOneShot(rollingSFX);
            rollSfxTimer = Time.time + 0.15f; 
        }
    }

    // ─── Player Movement ───────────────────────────────────────
    public void PlayWalk() 
    { 
        if (walkSFX != null && footstepSource != null && !footstepSource.isPlaying) 
        {
            footstepSource.clip = walkSFX;
            footstepSource.Play();
        } 
    }
    
    public void PlayDash() 
    { 
        if (dashSFX != null && footstepSource != null) 
        {
            // Dash overrides walking sound momentarily
            footstepSource.PlayOneShot(dashSFX);
        }
    }

    // ─── Machines (Looping) ────────────────────────────────────
    public void StartOven() 
    { 
        if (ovenLoopingSFX != null && ovenSource != null) 
        {
            ovenSource.clip = ovenLoopingSFX;
            ovenSource.loop = true;
            if (!ovenSource.isPlaying) ovenSource.Play();
        }
    }
    public void StopOven() { if (ovenSource != null) ovenSource.Stop(); }

    public void StartDoughMaker() 
    { 
        if (doughMakerLoopingSFX != null && doughMakerSource != null) 
        {
            doughMakerSource.clip = doughMakerLoopingSFX;
            doughMakerSource.loop = true;
            if (!doughMakerSource.isPlaying) doughMakerSource.Play();
        }
    }
    public void StopDoughMaker() { if (doughMakerSource != null) doughMakerSource.Stop(); }

    // ─── Helper ────────────────────────────────────────────────
    private void PlayOneShotSafe(AudioClip clip)
    {
        if (clip != null && oneShotSource != null)
        {
            oneShotSource.PlayOneShot(clip);
        }
    }
}
