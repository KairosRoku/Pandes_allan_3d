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
}
