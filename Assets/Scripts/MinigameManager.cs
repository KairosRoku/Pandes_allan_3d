using UnityEngine;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    public GameObject rollingPanel;
    public GameObject shapingPanel;
    public GameObject packagingPanel;

    private ProcessingTable activeTable;
    private PackagingStation activePackagingStation;
    private PlayerController activePlayer;

    public void StartRollingMinigame(ProcessingTable table)
    {
        activeTable = table;
        rollingPanel.SetActive(true);
        // Initialization logic for rolling...
    }

    public void FinishRolling()
    {
        rollingPanel.SetActive(false);
        activeTable.TransformDough(ItemType.RolledDough);
        activeTable = null;
    }

    public void StartShapingMinigame(ProcessingTable table)
    {
        activeTable = table;
        shapingPanel.SetActive(true);
        // Initialization logic for shaping...
    }

    public void FinishShaping()
    {
        shapingPanel.SetActive(false);
        activeTable.TransformDough(ItemType.ShapedDough);
        activeTable = null;
    }

    public void StartPackagingMinigame(PackagingStation station, PlayerController player)
    {
        activePackagingStation = station;
        activePlayer = player;
        packagingPanel.SetActive(true);
    }

    public void FinishPackaging(int count)
    {
        packagingPanel.SetActive(false);
        // Add count to paper bag
        GameObject bag = activePlayer.GetHeldItem();
        if (bag.TryGetComponent<ItemData>(out var data))
        {
            data.count += count;
        }
        activePackagingStation = null;
    }
}
