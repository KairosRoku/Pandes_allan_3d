using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KneadingMinigameUI : MonoBehaviour
{
    public static KneadingMinigameUI Instance;

    [Header("UI Elements")]
    public GameObject windowRoot;
    public Image progressBar;
    public TextMeshProUGUI instructionalText;
    
    [Header("Animation")]
    public RectTransform handImageLeft;
    public RectTransform handImageRight;
    public RectTransform doughImage;
    public float handMoveRange = 100f;

    [Header("Settings")]
    public float kneadingGoal = 100f;
    public float moveThreshold = 10f; // Minimum distance per movement to count
    public float progressPerMove = 2f;

    private float currentProgress = 0f;
    private bool isMinigameActive = false;
    private Vector3 lastMousePosition;
    private bool isMovingUp = true;
    private bool isMouseDown = false;

    private System.Action onCompleteCallback;
    private PlayerController playerRef;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        windowRoot.SetActive(false);
    }

    private void OnDisable()
    {
        if (SFXManager.Instance != null) SFXManager.Instance.StopKneading();
    }

    public void StartMinigame(PlayerController player, System.Action onComplete)
    {
        playerRef = player;
        onCompleteCallback = onComplete;
        
        currentProgress = 0f;
        isMinigameActive = true;
        
        // Freeze player and pause time
        player.enabled = false;
        Time.timeScale = 0f;

        if (GameManager.Instance != null)
            GameManager.Instance.ToggleHUD(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        windowRoot.SetActive(true);
        UpdateUI();
    }

    private void Update()
    {
        if (!isMinigameActive || windowRoot == null || !windowRoot.activeSelf) 
        {
            if (SFXManager.Instance != null) SFXManager.Instance.StopKneading();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMouseDown = false;
        }

        if (isMouseDown)
        {
            Vector3 currentMousePos = Input.mousePosition;
            float deltaY = currentMousePos.y - lastMousePosition.y;

            // SFX: Play if mouse is moving, else pause
            if (SFXManager.Instance != null)
            {
                if (Mathf.Abs(deltaY) > 1f) SFXManager.Instance.StartKneading();
                else SFXManager.Instance.StopKneading();
            }

            // Check for direction change and significant movement
            if (isMovingUp && deltaY < -moveThreshold)
            {
                // Switched from Up to Down
                isMovingUp = false;
                AddProgress();
                lastMousePosition = currentMousePos;
            }
            else if (!isMovingUp && deltaY > moveThreshold)
            {
                // Switched from Down to Up
                isMovingUp = true;
                AddProgress();
                lastMousePosition = currentMousePos;
            }

            // --- ANIMATION LOGIC ---
            float yOffset = Mathf.Clamp(deltaY * 5f, -handMoveRange, handMoveRange);
            Vector3 targetPos = new Vector3(0, yOffset, 0);

            if (handImageLeft != null)
                handImageLeft.localPosition = Vector3.Lerp(handImageLeft.localPosition, targetPos, Time.unscaledDeltaTime * 15f);
            
            if (handImageRight != null)
                handImageRight.localPosition = Vector3.Lerp(handImageRight.localPosition, targetPos, Time.unscaledDeltaTime * 15f);

            if (doughImage != null && !isMovingUp)
            {
                // Squash dough slightly when moving DOWN - Clamped to prevent flipping
                float targetSquash = 1.0f - (Mathf.Abs(deltaY) * 0.002f);
                targetSquash = Mathf.Clamp(targetSquash, 0.6f, 1.0f);
                
                doughImage.localScale = Vector3.Lerp(doughImage.localScale, new Vector3(1.1f, targetSquash, 1f), Time.unscaledDeltaTime * 10f);
            }
            else if (doughImage != null)
            {
                // Return to normal scale
                doughImage.localScale = Vector3.Lerp(doughImage.localScale, Vector3.one, Time.unscaledDeltaTime * 5f);
            }
        }
        else
        {
            if (SFXManager.Instance != null) SFXManager.Instance.StopKneading();
            
            // Return to IDLE poses
            if (handImageLeft != null) handImageLeft.localPosition = Vector3.Lerp(handImageLeft.localPosition, Vector3.zero, Time.unscaledDeltaTime * 5f);
            if (handImageRight != null) handImageRight.localPosition = Vector3.Lerp(handImageRight.localPosition, Vector3.zero, Time.unscaledDeltaTime * 5f);
            if (doughImage != null) doughImage.localScale = Vector3.Lerp(doughImage.localScale, Vector3.one, Time.unscaledDeltaTime * 2f);
        }
    }

    private void AddProgress()
    {
        currentProgress += progressPerMove;
        UpdateUI();

        if (currentProgress >= kneadingGoal)
        {
            CompleteMinigame();
        }
    }

    private void UpdateUI()
    {
        if (progressBar != null)
            progressBar.fillAmount = currentProgress / kneadingGoal;
    }

    private void CompleteMinigame()
    {
        isMinigameActive = false;
        windowRoot.SetActive(false);

        if (SFXManager.Instance != null) SFXManager.Instance.StopKneading();

        // Resume player and time
        if (playerRef != null)
            playerRef.enabled = true;

        if (GameManager.Instance != null)
            GameManager.Instance.ToggleHUD(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        onCompleteCallback?.Invoke();
    }
}
