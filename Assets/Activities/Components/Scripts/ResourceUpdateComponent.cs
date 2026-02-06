using UnityEngine;

[CreateAssetMenu(fileName = "ResourceUpdateComponent", menuName = "Club Fungal/Activities/Components/Resource Update")]
public class ResourceUpdateComponent : ActivityComponent
{
    [SerializeField] private ItemTemplate itemTemplate;
    [SerializeField] private int itemsPerUpdate = 1;
    [SerializeField] private float updateInterval = 1f;

    private float lastUpdateTime;

    public override void Update(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        if (Time.realtimeSinceStartup - lastUpdateTime >= updateInterval)
        {
            int unitCount = activityInstance.Units?.Count ?? 0;
            int totalItems = itemsPerUpdate * unitCount;

            if (totalItems > 0 && itemTemplate != null)
            {
                for (int i = 0; i < totalItems; i++)
                {
                    networkRun.Inventory.AddItem(itemTemplate);
                }
                Debug.Log($"Adding {totalItems} {itemTemplate.DisplayName} ({unitCount} units × {itemsPerUpdate}) to the network run");
            }

            lastUpdateTime = Time.realtimeSinceStartup;
        }
    }
}
