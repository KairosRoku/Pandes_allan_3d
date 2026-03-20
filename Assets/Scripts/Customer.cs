using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    public int pandesalRequirement;
    public float maxWaitTime = 45f;
    private float currentWaitTimer;
    private bool isBeingServed = false;
    
    [Header("UI")]
    public GameObject timerCanvas;
    public Image timerFillImage;
    public TMPro.TextMeshProUGUI orderText;

    [HideInInspector] public bool isVlogger = false;

    [Header("Animation & Movement")]
    public float walkSpeed = 3f;
    public Animator animator;
    
    private Vector3 targetPos;
    private Quaternion targetRot;
    private bool isMoving = false;
    private bool isLeaving = false;

    private CustomerWindow manager;

    public void Initialize(CustomerWindow window, int req)
    {
        manager = window;
        pandesalRequirement = req;
        currentWaitTimer = maxWaitTime;
        
        if (timerCanvas != null) timerCanvas.SetActive(false);
        if (orderText != null) orderText.text = req.ToString();
    }

    public void StartServing()
    {
        isBeingServed = true;
        if (timerCanvas != null) timerCanvas.SetActive(true);
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 400f * Time.deltaTime);

            if (animator != null) animator.SetBool("IsWalking", true);

            if (Vector3.Distance(transform.position, targetPos) < 0.05f)
            {
                transform.position = targetPos;
                transform.rotation = targetRot;
                isMoving = false;
                
                if (animator != null) animator.SetBool("IsWalking", false);

                if (isLeaving)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            if (animator != null) animator.SetBool("IsWalking", false);
        }

        if (isBeingServed && GameManager.Instance.isDayActive && !isLeaving)
        {
            currentWaitTimer -= Time.deltaTime;
            
            if (timerFillImage != null)
                timerFillImage.fillAmount = Mathf.Clamp01(currentWaitTimer / maxWaitTime);

            if (currentWaitTimer <= 0)
            {
                LeaveUnsatisfied();
            }
        }
    }

    private void LeaveUnsatisfied()
    {
        Debug.Log("[CUSTOMER] Wait time exceeded. Leaving...");
        if (isVlogger && GameManager.Instance != null)
        {
            GameManager.Instance.viralFailedDaysRemaining = 2;
            Debug.Log("[EVENT] Vlogger left unsatisfied. Flopped effect started.");
        }
        manager.OnCustomerLeft(this, false);
    }

    public void LeaveSatisfied()
    {
        if (isVlogger && GameManager.Instance != null)
        {
            GameManager.Instance.viralDaysRemaining = 5;
            Debug.Log("[EVENT] Vlogger satisfied! Viral effect started.");
        }
        manager.OnCustomerLeft(this, true);
    }

    public void MoveToTarget(Vector3 pos, Quaternion rot)
    {
        targetPos = pos;
        targetRot = rot;
        isMoving = true;
    }

    public void WalkAway(Vector3 exitPos)
    {
        if (timerCanvas != null) timerCanvas.SetActive(false);
        if (orderText != null) orderText.gameObject.SetActive(false);
        isBeingServed = false;
        
        targetPos = exitPos;
        
        // Rotate to look at the exit path
        Vector3 dir = (exitPos - transform.position);
        dir.y = 0; // maintain level
        if (dir.sqrMagnitude > 0.01f) 
            targetRot = Quaternion.LookRotation(dir.normalized);

        isMoving = true;
        isLeaving = true;
    }
}
