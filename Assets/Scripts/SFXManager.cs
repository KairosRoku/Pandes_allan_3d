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
    [Tooltip("For the main background music")]
    public AudioSource bgmSource;
    [Tooltip("For persistent kneading sound")]
    public AudioSource kneadingSource;
    [Tooltip("For persistent rolling sound")]
    public AudioSource rollingSource;

    [Header("Background Music")]
    public AudioClip backgroundMusicClip;

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
    public AudioClip[] walkSFX;
    public AudioClip dashSFX;

    private float rollSfxTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        PlayBGM();
    }

    // ─── Background Music ──────────────────────────────────────
    public void PlayBGM()
    {
        if (bgmSource != null && backgroundMusicClip != null)
        {
            bgmSource.clip = backgroundMusicClip;
            bgmSource.loop = true;
            if (!bgmSource.isPlaying) bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
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

    // ─── Minigames (Long Tracks) ───────────────────────────────
    public void StartKneading() 
    { 
        if (kneadingSFX != null && kneadingSource != null) 
        {
            if (kneadingSource.clip != kneadingSFX) kneadingSource.clip = kneadingSFX;
            if (!kneadingSource.isPlaying) kneadingSource.Play();
        }
    }
    public void StopKneading() { if (kneadingSource != null) kneadingSource.Stop(); }

    public void StartRolling() 
    { 
        if (rollingSFX != null && rollingSource != null) 
        {
            if (rollingSource.clip != rollingSFX) rollingSource.clip = rollingSFX;
            if (!rollingSource.isPlaying) rollingSource.Play();
        }
    }
    public void StopRolling() { if (rollingSource != null) rollingSource.Stop(); }
    
    // Legacy support to prevent errors if called
    public void PlayKneading() => StartKneading();
    public void PlayRolling() => StartRolling();

    // ─── Player Movement ───────────────────────────────────────
    public void PlayWalk() 
    { 
        if (walkSFX != null && walkSFX.Length > 0 && footstepSource != null && !footstepSource.isPlaying) 
        {
            // Pick a random footstep from the collection
            AudioClip clip = walkSFX[Random.Range(0, walkSFX.Length)];
            footstepSource.clip = clip;
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
