using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    None,
    Flour,
    Sugar,
    Water,
    Dough,
    DoughKnead,
    ShapedDough,
    Tray,
    TrayedShapedDough, // Shaped dough on a tray
    BakedPandesalTray, // Cooked tray
    BurntPandesalTray, // Burnt tray
    PaperBag
}

public class ItemData : MonoBehaviour
{
    public ItemType itemType;
    public int count = 1; // Used for paper bags or dough piles

    private bool initialized = false;

    private void Start()
    {
        // ONLY apply default stack amounts if we haven't been forcefully overridden by standard loaders
        if (!initialized)
        {
            if (itemType == ItemType.TrayedShapedDough || itemType == ItemType.BakedPandesalTray)
            {
                count = 30;
            }
            initialized = true;
        }
    }

    /// <summary>Call this when forcing a set count from the SaveSystem so Start() doesn't overwrite it.</summary>
    public void SetCountFromSave(int newCount)
    {
        count = newCount;
        initialized = true;
    }
}
