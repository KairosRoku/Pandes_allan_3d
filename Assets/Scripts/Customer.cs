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

    [HideInInspector] public bool isVlogger = false;

    private CustomerWindow manager;

    public void Initialize(CustomerWindow window, int req)
    {
        manager = window;
        pandesalRequirement = req;
        currentWaitTimer = maxWaitTime;
        
        if (timerCanvas != null) timerCanvas.SetActive(false);
    }

    public void StartServing()
    {
        isBeingServed = true;
        if (timerCanvas != null) timerCanvas.SetActive(true);
    }

    private void Update()
    {
        if (isBeingServed && GameManager.Instance.isDayActive)
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
}
