using UnityEngine;
using UnityEngine.EventSystems;

public class RollingMinigame : MonoBehaviour
{
    public float targetProgress = 100f;
    private float currentProgress = 0f;
    private bool isHolding = false;

    public MinigameManager manager;

    public void OnDown() => isHolding = true;
    public void OnUp() => isHolding = false;

    private void Update()
    {
        if (isHolding)
        {
            float delta = Mathf.Abs(Input.GetAxis("Mouse Y"));
            currentProgress += delta * 10f; // Scale speed
            
            if (currentProgress >= targetProgress)
            {
                currentProgress = 0;
                isHolding = false;
                manager.FinishRolling();
            }
        }
    }
}

public class ShapingMinigame : MonoBehaviour
{
    public float targetProgress = 360f * 3f; // 3 full circles
    private float currentProgress = 0f;
    private bool isHolding = false;
    private Vector2 center;
    private float lastAngle;

    public MinigameManager manager;

    public void OnDown()
    {
        isHolding = true;
        center = Input.mousePosition;
        lastAngle = 0;
    }
    public void OnUp() => isHolding = false;

    private void Update()
    {
        if (isHolding)
        {
            Vector2 mousePos = (Vector2)Input.mousePosition - center;
            if (mousePos.magnitude > 10f) // Threshold to avoid jitter at center
            {
                float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
                
                // Track angular delta
                if (lastAngle != 0)
                {
                    float delta = Mathf.DeltaAngle(lastAngle, angle);
                    currentProgress += Mathf.Abs(delta);
                }
                
                lastAngle = angle;
            }
            
            if (currentProgress >= targetProgress)
            {
                currentProgress = 0;
                isHolding = false;
                manager.FinishShaping();
            }
        }
    }
}
