using UnityEngine;

[CreateAssetMenu(fileName = "ResourceUpdateComponent", menuName = "Club Fungal/Activities/Components/Resource Update")]
public class ResourceUpdateComponent : ActivityComponent
{
    [SerializeField] private int sporesPerUpdate = 1;
    [SerializeField] private float updateInterval = 1f;

    private float lastUpdateTime;

    public override void Update(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        if (Time.realtimeSinceStartup - lastUpdateTime >= updateInterval)
        {
            int unitCount = activityInstance.Units?.Count ?? 0;
            int totalSpores = sporesPerUpdate * unitCount;

            if (totalSpores > 0)
            {
                Debug.Log($"Adding {totalSpores} spores ({unitCount} units × {sporesPerUpdate}) to the network run");
                networkRun.AddSpores(totalSpores);
            }

            lastUpdateTime = Time.realtimeSinceStartup;
        }
    }
}
