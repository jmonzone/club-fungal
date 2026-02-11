using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UpgradeActivityComponent", menuName = "Club Fungal/Activities/Components/Upgrade Activity")]
public class UpgradeActivityComponent : ActivityComponent
{
    [SerializeField] private Upgrade[] upgrades;
    [SerializeField] private List<UpgradeInstance> upgradeInstances = new List<UpgradeInstance>();

    [SerializeField] private ItemTemplate resourceCost;
    
    public Upgrade[] Upgrades => upgrades;
    public List<UpgradeInstance> UpgradeInstances => upgradeInstances;

    protected override void OnInitialize()
    {
        // Create upgrade instances from templates with scaling prices
        upgradeInstances = new List<UpgradeInstance>();
        for (int i = 0; i < upgrades.Length; i++)
        {
            if (upgrades[i] != null)
            {
                 // Price scales with upgrade index: base * (index + 1)
                int baseCost = 10;
                int scaledCost = baseCost * (i + 1);
                var price = new ResourceCost(resourceCost, scaledCost);
                Debug.Log($"Creating UpgradeInstance for {upgrades[i].UpgradeName} with price: {scaledCost} {resourceCost?.name}");
                var upgradeInstance = new UpgradeInstance(upgrades[i], new List<ResourceCost> { price });
                upgradeInstances.Add(upgradeInstance);
            }
        }
        
        Debug.Log($"UpgradeActivityComponent initialized with {upgradeInstances.Count} upgrade instances");
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Update logic called during network run
    }
}
