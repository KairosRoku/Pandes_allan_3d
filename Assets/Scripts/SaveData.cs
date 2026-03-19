using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// Atomic save record for a single interactable station in the scene.
// Identified by the GameObject's full scene path (reliable across runs
// as long as the hierarchy doesn't change).
// ─────────────────────────────────────────────────────────────────────────────

[Serializable]
public class StationSaveRecord
{
    /// <summary>Full hierarchy path, e.g. "Kitchen/Counter_01"</summary>
    public string scenePath;

    /// <summary>ItemType of the item currently on this station (None = empty).</summary>
    public ItemType itemType;

    /// <summary>Item count (for BakedPandesalTray, PaperBag, etc.). 0 = not applicable.</summary>
    public int itemCount;

    /// <summary>Bin / rack stock level (for DoughBin, IngredientRack, Dispenser).</summary>
    public int stockAmount;
}

[Serializable]
public class HeldItemRecord
{
    public ItemType itemType;
    public int      itemCount;
}

// ─────────────────────────────────────────────────────────────────────────────
// Full save-state data — all fields are JSON-serializable via JsonUtility.
// ─────────────────────────────────────────────────────────────────────────────

[Serializable]
public class SaveData
{
    // ── Economy ───────────────────────────────────────────────────────
    public int totalMoney          = 100;
    public int currentDay          = 1;
    public int totalGems           = 0;

    // ── Upgrades ──────────────────────────────────────────────────────
    public int doughMakingUpgradeLevel = 0;
    public int bakingUpgradeLevel      = 0;
    public int burnTimeUpgradeLevel    = 0;

    // ── World state ───────────────────────────────────────────────────

    /// <summary>
    /// One record per saveable station (DoughBin, Counter, IngredientRack, Dispenser …).
    /// </summary>
    public List<StationSaveRecord> stations = new List<StationSaveRecord>();

    /// <summary>What the player is holding when the game saved (may be None).</summary>
    public HeldItemRecord heldItem = new HeldItemRecord();
}
