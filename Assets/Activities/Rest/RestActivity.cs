using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RestActivity", menuName = "Club Fungal/Activities/Components/Rest")]
public class RestActivity : ActivityComponent
{
    [SerializeField] private Inventory inventory;
    private Dictionary<int, float> unitLastEnergyRestorationTimes = new Dictionary<int, float>();

    public Inventory Inventory => inventory;

    public override void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        base.Initialize(networkRun, activityInstance);
        inventory = new Inventory();
    }
    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        if (activityInstance.Units == null || activityInstance.Units.Count == 0)
            return;

        var currentTime = networkRun?.SimulationTime ?? UnityEngine.Time.realtimeSinceStartup;

        // Only restore energy if energy system is enabled
        var useEnergy = networkRun?.Settings?.useEnergySystem ?? true;
        if (!useEnergy) return;

        // Restore energy for each unit in the rest activity
        foreach (var unit in activityInstance.Units)
        {
            if (unit == null) continue;

            var unitKey = unit.GetHashCode();

            // Initialize timer if not present
            if (!unitLastEnergyRestorationTimes.ContainsKey(unitKey))
            {
                unitLastEnergyRestorationTimes[unitKey] = currentTime;
                continue;
            }

            // Restore energy based on elapsed time
            var timeSinceLastRestoration = currentTime - unitLastEnergyRestorationTimes[unitKey];
            var energyRestoration = networkRun?.Settings?.energyRestorationPerUpdate ?? 2f;
            unit.Energy += energyRestoration * (timeSinceLastRestoration / (networkRun?.Settings?.updateInterval ?? 1f));
            unitLastEnergyRestorationTimes[unitKey] = currentTime;
        }

        // Clean up removed units
        var activeUnitKeys = new System.Collections.Generic.HashSet<int>();
        foreach (var unit in activityInstance.Units)
        {
            if (unit != null)
                activeUnitKeys.Add(unit.GetHashCode());
        }

        var keysToRemove = new System.Collections.Generic.List<int>();
        foreach (var key in unitLastEnergyRestorationTimes.Keys)
        {
            if (!activeUnitKeys.Contains(key))
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
        {
            unitLastEnergyRestorationTimes.Remove(key);
        }
    }
}
