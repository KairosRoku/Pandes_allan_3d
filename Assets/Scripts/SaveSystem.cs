using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string GetPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, "save_slot_" + slot + ".json");
    }

    public static void Save(int slot, SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slot), json);
        Debug.Log($"[SAVE] Saved slot {slot} to {GetPath(slot)}");
    }

    public static SaveData Load(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Debug.Log($"[SAVE] No save found in slot {slot}. Returning new save data.");
            return new SaveData();
        }
    }

    public static bool DoesSlotExist(int slot)
    {
        return File.Exists(GetPath(slot));
    }

    public static void DeleteSlot(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    // Static variables to pass selection between scenes
    public static int SelectedSlot = 1;
}
