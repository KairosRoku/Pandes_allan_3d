using UnityEngine;

/// <summary>
/// Simple script to make an object (usually a world-space UI Canvas) face the main camera.
/// </summary>
public class Billboard : MonoBehaviour
{
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;

    private void LateUpdate()
    {
        if (Camera.main == null) return;

        // Get the direction to look at (facing away from the camera to look correct for sprites/UI)
        Vector3 targetDirection = transform.position + Camera.main.transform.forward;
        
        // Calculate the rotation
        Vector3 targetRotation = Quaternion.LookRotation(Camera.main.transform.forward).eulerAngles;

        // Apply locks if needed
        Vector3 currentRotation = transform.eulerAngles;
        if (lockX) targetRotation.x = currentRotation.x;
        if (lockY) targetRotation.y = currentRotation.y;
        if (lockZ) targetRotation.z = currentRotation.z;

        transform.rotation = Quaternion.Euler(targetRotation);
    }
}
