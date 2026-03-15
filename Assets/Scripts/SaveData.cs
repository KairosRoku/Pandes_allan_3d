using UnityEngine;
using System;

[Serializable]
public class SaveData
{
    public int totalMoney = 100;
    public int currentDay = 1;
    
    public int doughMakingUpgradeLevel = 0;
    public int bakingUpgradeLevel = 0;
    public int burnTimeUpgradeLevel = 0;

    // Optional: Add other persistent stats here (e.g., total items baked)
}
