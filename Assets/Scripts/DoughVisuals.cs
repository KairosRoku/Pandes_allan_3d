using UnityEngine;

public class DoughVisuals : MonoBehaviour
{
    public GameObject rawModel;
    public GameObject rolledModel;
    public GameObject shapedModel;
    public GameObject trayModel;

    private ItemData itemData;

    private void Awake()
    {
        itemData = GetComponent<ItemData>();
    }

    private void Start()
    {
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        if (itemData == null) return;

        // Hide all first
        if(rawModel) rawModel.SetActive(false);
        if(rolledModel) rolledModel.SetActive(false);
        if(shapedModel) shapedModel.SetActive(false);
        if(trayModel) trayModel.SetActive(false);

        // Show based on state
        switch (itemData.itemType)
        {
            case ItemType.Dough:
                if(rawModel) rawModel.SetActive(true);
                break;
            case ItemType.RolledDough:
                if(rolledModel) rolledModel.SetActive(true);
                break;
            case ItemType.ShapedDough:
                if(shapedModel) shapedModel.SetActive(true);
                break;
            case ItemType.TrayedShapedDough:
                if(shapedModel) shapedModel.SetActive(true);
                if(trayModel) trayModel.SetActive(true);
                break;
        }
    }
}
