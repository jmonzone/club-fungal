using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceUpdateComponent", menuName = "Club Fungal/Activities/Components/Resource Update")]
public class ResourceUpdateComponent : ActivityComponent
{
    [SerializeField] private ItemTemplate itemTemplate;
    [SerializeField] private int itemsPerUpdate = 1;
    [SerializeField] private float updateInterval = 1f;

    private Dictionary<int, float> unitLastUpdateTimes = new Dictionary<int, float>();
    private Dictionary<int, float> unitLastEnergyDepletionTimes = new Dictionary<int, float>();

    public ItemTemplate ItemTemplate => itemTemplate;
    public float UpdateInterval => updateInterval;
    public int ItemsPerUpdate => itemsPerUpdate;

    public float GetSpeedBonus(UnitInstance unit, ActivityInstance activity = null)
    {
        if (unit?.Species == null || itemTemplate == null)
            return 1f;

        var unitType = unit.Species.Type;

        // Get speed bonus from unit type for this resource
        float speedBonus = unitType?.GetSpeedBonusForResource(itemTemplate) ?? 1f;

        // Add skill level bonus to speed
        if (activity?.Template?.PrimarySkill != null && unit?.Skills != null)
        {
            if (unit.Skills.TryGetValue(activity.Template.PrimarySkill, out var skillInstance))
            {
                // Each skill level adds 10% speed bonus
                speedBonus += skillInstance.Level * 0.1f;
            }
        }

        // Apply exhaustion debuff (halve speed)
        if (unit.IsExhausted)
        {
            speedBonus *= 0.5f;
        }

        return speedBonus;
    }

    public float GetEffectiveInterval(UnitInstance unit, ActivityInstance activity = null, NetworkRun networkRun = null)
    {
        if (unit?.Species == null || itemTemplate == null)
            return updateInterval;

        var speedBonus = GetSpeedBonus(unit, activity);
        return updateInterval / speedBonus;
    }

    public float GetUnitProgress(UnitInstance unit, ActivityInstance activity = null, NetworkRun networkRun = null)
    {
        if (unit == null) return 0f;

        // Check if collecting to global inventory
        var collectToGlobal = networkRun?.Settings?.unitsCollectToGlobalInventory ?? false;

        // If collecting to unit inventory and it's full, don't show progress
        if (!collectToGlobal && unit.Inventory != null && unit.Inventory.IsFull)
        {
            return 0f;
        }

        // When collecting to global, don't check inventory limits - keep collecting

        var unitKey = unit.GetHashCode();
        if (!unitLastUpdateTimes.ContainsKey(unitKey))
        {
            unitLastUpdateTimes[unitKey] = networkRun?.SimulationTime ?? Time.realtimeSinceStartup;
            return 0f;
        }

        var effectiveInterval = GetEffectiveInterval(unit, activity, networkRun);
        var currentTime = networkRun?.SimulationTime ?? Time.realtimeSinceStartup;
        var timeSinceLastUpdate = currentTime - unitLastUpdateTimes[unitKey];
        return Mathf.Clamp01(timeSinceLastUpdate / effectiveInterval);
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        if (activityInstance.Units == null || activityInstance.Units.Count == 0)
            return;

        // Update each unit independently
        var unitsToUpdate = new List<UnitInstance>(activityInstance.Units);
        foreach (var unit in unitsToUpdate)
        {
            if (unit == null) continue;

            var unitKey = unit.GetHashCode();
            var currentTime = networkRun?.SimulationTime ?? Time.realtimeSinceStartup;

            // Initialize timers if not present
            if (!unitLastUpdateTimes.ContainsKey(unitKey))
            {
                unitLastUpdateTimes[unitKey] = currentTime;
                unitLastEnergyDepletionTimes[unitKey] = currentTime;
                continue;
            }

            // Check and update exhaustion status FIRST based on current energy (only if energy system is enabled)
            var useEnergy = networkRun?.Settings?.useEnergySystem ?? true;
            if (useEnergy)
            {
                if (unit.Energy <= 0f)
                {
                    unit.IsExhausted = true;
                }
                else if (unit.Energy > 20f)
                {
                    // Remove exhaustion when energy recovers above 20%
                    unit.IsExhausted = false;
                }

                // Deplete energy based on elapsed time since last energy check
                if (unitLastEnergyDepletionTimes.ContainsKey(unitKey))
                {
                    var timeSinceLastEnergyDepletion = currentTime - unitLastEnergyDepletionTimes[unitKey];
                    var energyDepletion = networkRun?.Settings?.energyDepletionPerUpdate ?? 1f;
                    unit.Energy -= energyDepletion * (timeSinceLastEnergyDepletion / (networkRun?.Settings?.updateInterval ?? 1f));
                    unitLastEnergyDepletionTimes[unitKey] = currentTime;
                }
            }

            var effectiveInterval = GetEffectiveInterval(unit, activityInstance, networkRun);
            var timeSinceLastUpdate = currentTime - unitLastUpdateTimes[unitKey];
            if (timeSinceLastUpdate >= effectiveInterval)
            {
                var collectToGlobal = networkRun?.Settings?.unitsCollectToGlobalInventory ?? false;

                // Check if inventory has space before collecting
                if (itemTemplate != null)
                {
                    var targetInventory = collectToGlobal ? networkRun?.Inventory : unit.Inventory;

                    if (targetInventory == null)
                    {
                        unitLastUpdateTimes[unitKey] = currentTime;
                        continue;
                    }

                    // Only check if inventory is full when collecting to unit inventory
                    if (!collectToGlobal && targetInventory.IsFull)
                    {
                        // Inventory is full, keep progress at 0% (reset timer to current time)
                        unitLastUpdateTimes[unitKey] = currentTime;
                        continue;
                    }

                    for (int i = 0; i < itemsPerUpdate; i++)
                    {
                        targetInventory.AddItem(itemTemplate);
                    }
                    // Debug.Log($"{unit.DisplayName} collected {itemsPerUpdate}x {itemTemplate.DisplayName} to {(collectToGlobal ? "global" : "unit")} inventory");
                }

                // Add XP to the unit's skill
                if (activityInstance?.Template?.PrimarySkill != null && unit?.Skills != null)
                {
                    if (unit.Skills.TryGetValue(activityInstance.Template.PrimarySkill, out var skillInstance))
                    {
                        skillInstance.IncreaseSkillXP(unit, 10);
                    }
                }

                unitLastUpdateTimes[unitKey] = networkRun?.SimulationTime ?? Time.realtimeSinceStartup;
            }
        }

        // Clean up removed units
        var activeUnitKeys = new HashSet<int>();
        foreach (var u in activityInstance.Units)
        {
            if (u != null)
                activeUnitKeys.Add(u.GetHashCode());
        }

        var keysToRemove = new List<int>();
        foreach (var key in unitLastUpdateTimes.Keys)
        {
            if (!activeUnitKeys.Contains(key))
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
        {
            unitLastUpdateTimes.Remove(key);
            unitLastEnergyDepletionTimes.Remove(key);
        }
    }
}
