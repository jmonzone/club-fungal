using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceUpdateComponent", menuName = "Club Fungal/Activities/Components/Resource Update")]
public class ResourceUpdateComponent : ActivityComponent
{
    [SerializeField] private ItemTemplate itemTemplate;
    [SerializeField] private int itemsPerUpdate = 1;
    [SerializeField] private float updateInterval = 1f;
    [SerializeField] private float aquaSpeedBonus = 2f; // Speed multiplier for Aqua units collecting fish
    [SerializeField] private float pawSpeedBonus = 2f; // Speed multiplier for Paw units collecting spores
    [SerializeField] private float skySpeedBonus = 2f; // Speed multiplier for Sky units (future use)

    private Dictionary<int, float> unitLastUpdateTimes = new Dictionary<int, float>();

    public ItemTemplate ItemTemplate => itemTemplate;
    public float UpdateInterval => updateInterval;
    public int ItemsPerUpdate => itemsPerUpdate;

    public float GetSpeedBonus(UnitInstance unit, ActivityInstance activity = null)
    {
        if (unit?.Species == null || itemTemplate == null)
            return 1f;

        var unitType = unit.Species.Type;
        var itemId = itemTemplate.Id?.ToLower();

        // Check for type matches and apply appropriate bonus
        float speedBonus = 1f;
        if (unitType == UnitType.Aqua && itemId?.Contains("fish") == true)
            speedBonus = aquaSpeedBonus;
        else if (unitType == UnitType.Paw && itemId?.Contains("spore") == true)
            speedBonus = pawSpeedBonus;
        else if (unitType == UnitType.Sky)
            speedBonus = skySpeedBonus; // Reserved for future sky-type resources

        // Add skill level bonus to speed
        if (activity?.Template?.PrimarySkill != null && unit?.Skills != null)
        {
            if (unit.Skills.TryGetValue(activity.Template.PrimarySkill, out var skillInstance))
            {
                // Each skill level adds 10% speed bonus
                speedBonus += skillInstance.Level * 0.1f;
            }
        }

        return speedBonus;
    }

    public float GetEffectiveInterval(UnitInstance unit, ActivityInstance activity = null)
    {
        if (unit?.Species == null || itemTemplate == null)
            return updateInterval;

        var speedBonus = GetSpeedBonus(unit, activity);
        return updateInterval / speedBonus;
    }

    public float GetUnitProgress(UnitInstance unit, ActivityInstance activity = null)
    {
        if (unit == null) return 0f;
        var unitKey = unit.GetHashCode();
        if (!unitLastUpdateTimes.ContainsKey(unitKey))
        {
            unitLastUpdateTimes[unitKey] = Time.realtimeSinceStartup;
            return 0f;
        }

        var effectiveInterval = GetEffectiveInterval(unit, activity);
        var timeSinceLastUpdate = Time.realtimeSinceStartup - unitLastUpdateTimes[unitKey];
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
            if (!unitLastUpdateTimes.ContainsKey(unitKey))
            {
                unitLastUpdateTimes[unitKey] = Time.realtimeSinceStartup;
                continue;
            }

            var effectiveInterval = GetEffectiveInterval(unit, activityInstance);
            var timeSinceLastUpdate = Time.realtimeSinceStartup - unitLastUpdateTimes[unitKey];
            if (timeSinceLastUpdate >= effectiveInterval)
            {
                if (itemTemplate != null)
                {
                    for (int i = 0; i < itemsPerUpdate; i++)
                    {
                        unit.Inventory.AddItem(itemTemplate);
                    }
                    // Debug.Log($"{unit.DisplayName} collected {itemsPerUpdate}x {itemTemplate.DisplayName}");
                }

                // Add XP to the unit's skill
                if (activityInstance?.Template?.PrimarySkill != null && unit?.Skills != null)
                {
                    if (unit.Skills.TryGetValue(activityInstance.Template.PrimarySkill, out var skillInstance))
                    {
                        skillInstance.IncreaseSkillXP(unit, 10);
                    }
                }

                unitLastUpdateTimes[unitKey] = Time.realtimeSinceStartup;
            }
        }

        // Clean up removed units
        var activeUnitKeys = new HashSet<int>();
        foreach (var unit in activityInstance.Units)
        {
            if (unit != null)
                activeUnitKeys.Add(unit.GetHashCode());
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
        }
    }
}
