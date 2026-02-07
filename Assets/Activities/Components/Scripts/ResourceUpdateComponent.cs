using UnityEngine;

[CreateAssetMenu(fileName = "ResourceUpdateComponent", menuName = "Club Fungal/Activities/Components/Resource Update")]
public class ResourceUpdateComponent : ActivityComponent
{
    [SerializeField] private ItemTemplate itemTemplate;
    [SerializeField] private int itemsPerUpdate = 1;
    [SerializeField] private float updateInterval = 1f;

    private float lastUpdateTime;

    public ItemTemplate ItemTemplate => itemTemplate;
    public float UpdateInterval => updateInterval;
    public int ItemsPerUpdate => itemsPerUpdate;
    public float TimeSinceLastUpdate => Time.realtimeSinceStartup - lastUpdateTime;
    public float Progress => Mathf.Clamp01(TimeSinceLastUpdate / updateInterval);

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        if (Time.realtimeSinceStartup - lastUpdateTime >= updateInterval)
        {
            int unitCount = activityInstance.Units?.Count ?? 0;

            if (unitCount > 0 && itemTemplate != null)
            {
                foreach (var unit in activityInstance.Units)
                {
                    if (unit != null)
                    {
                        for (int i = 0; i < itemsPerUpdate; i++)
                        {
                            unit.Inventory.AddItem(itemTemplate);
                        }
                    }
                }
                Debug.Log($"Adding {itemsPerUpdate}x {itemTemplate.DisplayName} to {unitCount} unit inventories");
            }

            lastUpdateTime = Time.realtimeSinceStartup;
        }
    }
}
