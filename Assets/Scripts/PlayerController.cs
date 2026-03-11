using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 20f;

    [Header("Interaction")]
    public float interactionDistance = 2f;
    public Transform holdPoint;
    public LayerMask interactionLayer;
    public InteractionRod interactionRod;
    public bool autoInteractOnTouch = true;

    [Header("Dash")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    private Vector2 moveInput;
    private CharacterController characterController;
    private GameObject heldItem;
    private IInteractable currentInteractable;
    private bool isDashing = false;
    private float lastDashTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        // Ensure we have a rod if assigned
        if (interactionRod == null)
        {
            interactionRod = GetComponentInChildren<InteractionRod>();
        }
    }

    private void Update()
    {
        if (isDashing) return;

        HandleInput();
        HandleMovement();
        HandleInteractionDetection();
    }

    private void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Direct Keyboard API to avoid Project Setting errors
        float x = 0;
        float y = 0;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1;
        
        moveInput = new Vector2(x, y).normalized;

        // Interaction Key (E)
        if (keyboard.eKey.wasPressedThisFrame)
        {
            ExecuteInteraction();
        }

        // Dash Key (Left Shift)
        if (keyboard.leftShiftKey.wasPressedThisFrame && !isDashing && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(PerformDash());
        }
    }

    private System.Collections.IEnumerator PerformDash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        
        Vector3 dashDir = transform.forward;
        if (dashDir == Vector3.zero) dashDir = Vector3.forward; // Fallback

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            characterController.Move(dashDir * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    private void ExecuteInteraction()
    {
        // Refresh detection immediately to ensure accuracy
        if (interactionRod != null)
        {
            currentInteractable = interactionRod.GetNearestInteractable();
        }

        string targetName = currentInteractable != null ? (currentInteractable as MonoBehaviour).gameObject.name : "NONE";
        string targetType = currentInteractable != null ? currentInteractable.GetType().Name : "N/A";
        string heldName = IsHoldingItem() ? heldItem.name : "EMPTY HANDS";

        Debug.Log($"[INTERACT] Key: E | Target: {targetName} ({targetType}) | Holding: {heldName}");

        if (currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
        else if (IsHoldingItem())
        {
            Debug.Log("[INTERACT] Cannot drop items here. Must be placed on a Counter or in the DoughMaker.");
        }
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        if (move.magnitude > 0.1f)
        {
            // Move character in world space
            characterController.Move(move * moveSpeed * Time.deltaTime);

            // Rotate character model to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * 100f * Time.deltaTime);
        }

        // Apply constant gravity
        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }

    private void HandleInteractionDetection()
    {
        if (interactionRod != null)
        {
            currentInteractable = interactionRod.GetNearestInteractable();

            // If auto-interact is on and we are touching something while empty-handed
            if (autoInteractOnTouch && currentInteractable != null && !IsHoldingItem())
            {
                // ONLY auto-interact with things that GIVE items (Supply stations)
                // We do NOT want to auto-interact with machines (DoughMaker) or counters
                if (currentInteractable is IngredientRack || currentInteractable is DoughBin || currentInteractable is Dispenser || currentInteractable is TrayBin)
                {
                    currentInteractable.Interact(this);
                }
            }
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, interactionDistance, interactionLayer))
            {
                currentInteractable = hit.collider.GetComponent<IInteractable>();
            }
            else
            {
                currentInteractable = null;
            }
        }
    }

    public bool IsHoldingItem() => heldItem != null;
    public GameObject GetHeldItem() => heldItem;
    public IInteractable GetCurrentInteractable() => currentInteractable;

    public void PickUpItem(GameObject item)
    {
        if (heldItem != null) return;

        heldItem = item;
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
        heldItem.transform.localScale = Vector3.one; // Reset scale to avoid inheritance issues

        // Disable physics if any
        if (heldItem.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }
        if (heldItem.TryGetComponent<Collider>(out var col))
        {
            col.enabled = false;
        }
    }

    public GameObject RemoveHeldItem()
    {
        GameObject item = heldItem;
        heldItem = null;
        if (item != null)
        {
            item.transform.SetParent(null);
            if (item.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;
            }
            if (item.TryGetComponent<Collider>(out var col))
            {
                col.enabled = true;
            }
        }
        return item;
    }
}

