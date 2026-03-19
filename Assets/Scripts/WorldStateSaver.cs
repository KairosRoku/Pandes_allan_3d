using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place ONE of these on a persistent GameObject in your game scene (e.g. "GameManager" or "WorldRoot").
///
/// It orchestrates the save/load of all ISaveable stations and the player's held item.
///
/// INSPECTOR SETUP:
///   • Assign the PlayerController to the 'player' field.
///   • Fill the 'prefabRegistry' list matching each ItemType to its prefab.
///     (You only need entries for types that get placed on counters / held by the player.)
/// </summary>
public class WorldStateSaver : MonoBehaviour
{
    public static WorldStateSaver Instance;

    [Header("References")]
    public PlayerController player;

    [Header("Item Prefab Registry")]
    [Tooltip("Maps every spawnable ItemType to its prefab for use during load.")]
    public List<ItemPrefabEntry> prefabRegistry = new List<ItemPrefabEntry>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────

    /// <summary>
    /// Walks all ISaveable objects in the scene and writes their state into
    /// the given SaveData, plus the player's held item.
    /// </summary>
    public void CaptureWorldState(SaveData data)
    {
        data.stations.Clear();

        var saveables = FindObjectsOfType<MonoBehaviour>(includeInactive: true);
        foreach (var mb in saveables)
        {
            if (mb is ISaveable saveable)
            {
                var record = saveable.CaptureState();
                if (record != null)
                    data.stations.Add(record);
            }
        }

        // Save what the player is currently holding
        data.heldItem = new HeldItemRecord();
        if (player != null && player.IsHoldingItem())
        {
            var held = player.GetHeldItem();
            var itemData = held.GetComponentInChildren<ItemData>();
            if (itemData != null)
            {
                data.heldItem.itemType  = itemData.itemType;
                data.heldItem.itemCount = itemData.count;
            }
        }

        Debug.Log($"[SAVE] Captured {data.stations.Count} station records.");
    }

    /// <summary>
    /// Distributes loaded records to matching ISaveable objects in the scene,
    /// then restores the player's held item.
    /// </summary>
    public void RestoreWorldState(SaveData data)
    {
        if (data == null) return;

        // Build lookup from scenePath → record for O(1) access
        var lookup = new Dictionary<string, StationSaveRecord>();
        foreach (var record in data.stations)
        {
            if (!string.IsNullOrEmpty(record.scenePath))
                lookup[record.scenePath] = record;
        }

        var saveables = FindObjectsOfType<MonoBehaviour>(includeInactive: true);
        int restored = 0;
        foreach (var mb in saveables)
        {
            if (mb is ISaveable saveable)
            {
                string key = GetScenePath(mb.gameObject);
                if (lookup.TryGetValue(key, out var record))
                {
                    saveable.RestoreState(record);
                    restored++;
                }
                else
                {
                    // Station exists in scene but not in save — restore to empty/default
                    saveable.RestoreState(new StationSaveRecord
                    {
                        scenePath   = key,
                        itemType    = ItemType.None,
                        itemCount   = 0,
                        stockAmount = 0
                    });
                }
            }
        }

        // Restore held item
        if (player != null && data.heldItem != null && data.heldItem.itemType != ItemType.None)
        {
            GameObject prefab = GetPrefab(data.heldItem.itemType);
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, player.holdPoint.position, player.holdPoint.rotation);
                var itemData = obj.GetComponentInChildren<ItemData>();
                if (itemData != null)
                {
                    itemData.itemType = data.heldItem.itemType;
                    itemData.count    = data.heldItem.itemCount;
                }
                player.PickUpItem(obj);
            }
        }

        Debug.Log($"[LOAD] Restored {restored} station records.");
    }

    // ── Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full hierarchy path for use as a stable unique key.
    /// e.g. "Kitchen/Counter_01"
    /// </summary>
    public static string GetScenePath(GameObject go)
    {
        string path = go.name;
        Transform t = go.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }

    /// <summary>Returns the prefab associated with an ItemType, or null if not registered.</summary>
    public GameObject GetPrefab(ItemType type)
    {
        foreach (var entry in prefabRegistry)
        {
            if (entry.itemType == type)
                return entry.prefab;
        }
        Debug.LogWarning($"[WorldStateSaver] No prefab registered for ItemType.{type}");
        return null;
    }
}

/// <summary>Inspector-serializable ItemType→Prefab mapping.</summary>
[System.Serializable]
public class ItemPrefabEntry
{
    public ItemType      itemType;
    public GameObject    prefab;
}
