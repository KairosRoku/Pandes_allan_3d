using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShapingMinigameUI : MonoBehaviour
{
    public static ShapingMinigameUI Instance;

    [Header("UI Elements")]
    public GameObject windowRoot;
    public Image progressBar;
    public RectTransform circularArea; // Suggestive area to rotate around

    [Header("Settings")]
    public float shapingGoal = 100f;
    public float progressPerDegree = 0.1f;

    private float currentProgress = 0f;
    private bool isMinigameActive = false;
    private float lastAngle;
    private bool isMouseDown = false;
    private Vector2 center;

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
        
        // Calculate center of the area
        if (circularArea != null)
            center = circularArea.position;
        else
            center = new Vector2(Screen.width / 2f, Screen.height / 2f);

        UpdateUI();
    }

    private void Update()
    {
        if (!isMinigameActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
            lastAngle = GetMouseAngle();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMouseDown = false;
        }

        if (isMouseDown)
        {
            float currentAngle = GetMouseAngle();
            float delta = Mathf.Abs(Mathf.DeltaAngle(lastAngle, currentAngle));
            
            AddProgress(delta * progressPerDegree);
            lastAngle = currentAngle;
        }
    }

    private float GetMouseAngle()
    {
        Vector2 mousePos = Input.mousePosition;
        return Mathf.Atan2(mousePos.y - center.y, mousePos.x - center.x) * Mathf.Rad2Deg;
    }

    private void AddProgress(float amount)
    {
        currentProgress += amount;
        UpdateUI();

        if (SFXManager.Instance != null && amount > 0.1f) SFXManager.Instance.PlayRolling();

        if (currentProgress >= shapingGoal)
        {
            CompleteMinigame();
        }
    }

    private void UpdateUI()
    {
        if (progressBar != null)
            progressBar.fillAmount = currentProgress / shapingGoal;
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
