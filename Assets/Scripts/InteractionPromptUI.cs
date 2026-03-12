using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public PlayerController player;
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;

    private void Update()
    {
        IInteractable interactable = player.GetCurrentInteractable();
        if (interactable != null)
        {
            promptPanel.SetActive(true);
            promptText.text = "[E] " + interactable.GetInteractText(player);
        }
        else
        {
            promptPanel.SetActive(false);
        }
    }
}
