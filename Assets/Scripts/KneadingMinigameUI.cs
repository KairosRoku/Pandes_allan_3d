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
        if (!isMinigameActive) return;

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
        }
    }

    private void AddProgress()
    {
        currentProgress += progressPerMove;
        UpdateUI();

        if (SFXManager.Instance != null) SFXManager.Instance.PlayKneading();

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
