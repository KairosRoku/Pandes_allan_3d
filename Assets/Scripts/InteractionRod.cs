using UnityEngine;
using System.Collections.Generic;

public class InteractionRod : MonoBehaviour
{
    private System.Collections.Generic.HashSet<IInteractable> currentOverlaps = new System.Collections.Generic.HashSet<IInteractable>();

    private void OnTriggerEnter(Collider other)
    {
        // Use GetComponentInParent so if the rod hits the dough (which is a child
        // of itemPlacementPoint, which is a child of the counter), it correctly
        // bubbles up and finds the Counter/ProcessingTable.
        IInteractable interactable = other.GetComponentInParent<IInteractable>();

        if (interactable != null)
        {
            currentOverlaps.Add(interactable);
            MonoBehaviour mb = interactable as MonoBehaviour;
            Debug.Log($"[ROD] Detected Interactable: {mb.gameObject.name} (Tag: {mb.gameObject.tag})");
        }
        else
        {
            // Helpful debug to see what is "blocking" the rod
            Debug.Log($"[ROD] Touching non-interactable: {other.gameObject.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponentInParent<IInteractable>();

        if (interactable != null)
        {
            currentOverlaps.Remove(interactable);
        }
    }

    public IInteractable GetNearestInteractable()
    {
        IInteractable nearest = null;
        float minDistance = float.MaxValue;

        // Clean up removed/destroyed objects
        currentOverlaps.RemoveWhere(i => i == null || (i as MonoBehaviour) == null || !(i as MonoBehaviour).gameObject.activeInHierarchy);

        foreach (var interactable in currentOverlaps)
        {
            float dist = Vector3.Distance(transform.position, (interactable as MonoBehaviour).transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = interactable;
            }
        }

        return nearest;
    }
}
