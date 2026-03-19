using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Automatically hooks up a click sound to every UI Button in the scene.
/// Just drop this script onto the GameManager or any empty GameObject in your scene!
/// </summary>
public class AutoButtonSFX : MonoBehaviour
{
    private void Start()
    {
        // Find all buttons in the scene, including those on hidden/inactive UI panels!
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        
        foreach (Button btn in buttons)
        {
            // Ensure we aren't hooking up prefab assets directly, only scene objects
            if (btn.gameObject.hideFlags == HideFlags.NotEditable || btn.gameObject.hideFlags == HideFlags.HideAndDontSave)
                continue;

            if (btn.gameObject.scene.isLoaded)
            {
                btn.onClick.AddListener(() =>
                {
                    if (SFXManager.Instance != null)
                        SFXManager.Instance.PlayButtonPress();
                });
            }
        }
    }
}
