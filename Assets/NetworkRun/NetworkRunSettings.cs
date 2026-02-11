using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ResourceCollectionMode
{
    UnitInventory,      // Resources go to individual unit inventories
    GlobalInventory     // Resources go directly to global inventory
}

[Serializable]
public class ZoneCostOverride
{
    public int zoneIndex; // Zone index (0-based, relative to hideZoneAfterIndex)
    [Tooltip("Spore cost override. Set to -1 to use formula, or >= 0 to override.")]
    public int cost = -1; // Required cost to reveal this zone (-1 = use formula)
    [Tooltip("Optional additional resource cost (e.g., fish, wood). Leave empty for spores only.")]
    public ResourceCost resourceCost; // Additional resource requirement
}

[Serializable]
public class SkillToggle
{
    public Skill skill;
    public bool enabled = true;

    public SkillToggle(Skill skill, bool enabled = true)
    {
        this.skill = skill;
        this.enabled = enabled;
    }
}

[CreateAssetMenu(fileName = "NetworkRunSettings", menuName = "Club Fungal/Network Run/Settings")]
public class NetworkRunSettings : ScriptableObject
{
    // Add settings fields as needed
    public int defaultPartySize = 3;
    public float updateInterval = 1.0f;
    public bool debugMode = false;
    public float speedMultiplier = 1.0f; // Simulation speed (1.0 = normal, 2.0 = 2x speed, etc.)
    public int minimumActivities = 1; // Minimum number of resource activities per room
    public bool useEnergySystem = true; // Enable/disable energy depletion and restoration
    public float energyDepletionPerUpdate = 1f; // How much energy is depleted per resource collection
    public float energyRestorationPerUpdate = 2f; // How much energy is restored per update while resting
    public ItemTemplate sporesItem; // Item required for door unlocks
    public ItemTemplate fishItem; // Food item for resting
    public ZoneOcclusion zoneOcclusionTemplate; // Template for zone occlusion components
    public bool enableZoneOcclusion = true; // Enable/disable zone occlusion for resource activities
    public int hideZoneAfterIndex = 0; // Only hide resource components starting from this index (0 = hide all)
    [Range(0.1f, 10f)]
    public float zoneRevealCostScale = 1f; // Multiplier for zone reveal costs (1.0 = normal, 0.5 = half cost, 2.0 = double cost)
    public ResourceCollectionMode zoneContributionMode = ResourceCollectionMode.UnitInventory; // Where zone reveal contributions come from

    [Header("Zone Cost Overrides")]
    [Tooltip("Explicit costs for specific zone indices. Overrides formula calculation. Use for fine-tuning specific zones.")]
    public List<ZoneCostOverride> zoneCostOverrides = new List<ZoneCostOverride>();

    public bool showAllHiddenZones = false; // If true, show all hidden zones at once. If false, only show one hidden zone at a time
    public bool enableDoorActivities = true; // Enable/disable door unlock activities
    public ResourceCollectionMode resourceCollectionMode = ResourceCollectionMode.UnitInventory; // Where collected resources go
    public bool showUnitInventoryItems = true; // Show unit inventory items on unit cards
    public bool showUnitInventoryButton = true; // Show "View Data" button on unit cards
    public bool showViewUnitButton = true; // Show "View Unit" button on rest activity unit cards
    public bool showResourceSkillLevel = false; // Show skill level and XP progress on resource unit cards
    public ActivityReference restActivity; // Rest activity
    public ActivityReference sporeActivity; // Rest activity
    public ActivityReference doorActivity; // Door/unlock activity
    public List<ActivityReference> resourceActivities; // Resource-producing activities (filtered)

    [Header("Room Activities")]
    [Tooltip("Master list of activity templates. All rooms use these activities in the same order.")]
    public List<ActivityReference> activities = new List<ActivityReference>();

    [Header("Skills")]
    [SerializeField] private List<SkillToggle> skills = new List<SkillToggle>(); // Skills that can be toggled on/off

    [Header("Initial Inventory")]
    [Tooltip("Items to add to the global inventory when starting a new network run.")]
    public List<Inventory.ItemStack> initialInventory = new List<Inventory.ItemStack>();

    // Add more settings here

    /// <summary>
    /// Check if a skill is enabled in the game settings
    /// </summary>
    public bool IsSkillEnabled(Skill skill)
    {
        if (skill == null) return false;

        var skillToggle = skills.FirstOrDefault(s => s.skill == skill);
        if (skillToggle != null)
        {
            return skillToggle.enabled;
        }

        // If skill not in list, assume enabled
        return true;
    }

    /// <summary>
    /// Get all enabled skills
    /// </summary>
    public List<Skill> GetEnabledSkills()
    {
        return skills.Where(s => s.enabled && s.skill != null).Select(s => s.skill).ToList();
    }

    /// <summary>
    /// Get all configured skills (enabled or disabled)
    /// </summary>
    public List<SkillToggle> GetAllSkillToggles()
    {
        return skills;
    }

    /// <summary>
    /// Get room activities instantiated from the master activities list
    /// </summary>
    public List<ActivityInstance> GetRoomActivities(NetworkRun networkRun)
    {
        var instances = new List<ActivityInstance>();

        if (activities != null && activities.Count > 0)
        {
            for (int i = 0; i < activities.Count; i++)
            {
                var activityRef = activities[i];
                if (activityRef != null)
                {
                    // Create a runtime copy of the activity reference
                    var activityRefCopy = Instantiate(activityRef);
                    activityRefCopy.name = activityRef.name;

                    // Add ZoneOcclusion component if enabled, template exists, and index is >= threshold
                    if (enableZoneOcclusion && zoneOcclusionTemplate != null && sporesItem != null && i >= hideZoneAfterIndex)
                    {
                        // Copy existing components
                        var existingComponents = new List<ActivityComponent>();
                        if (activityRefCopy.Components != null)
                        {
                            foreach (var comp in activityRefCopy.Components)
                            {
                                if (comp != null)
                                {
                                    var compCopy = Instantiate(comp);
                                    compCopy.name = comp.name;
                                    existingComponents.Add(compCopy);
                                }
                            }
                        }

                        // Create ZoneOcclusion component
                        var zoneOcclusion = Instantiate(zoneOcclusionTemplate);
                        zoneOcclusion.name = "ZoneOcclusion";

                        // Initialize zone occlusion with the first existing component and calculated cost
                        // Use +2 offset to match door unlock formula (first room level = 2 = 83 XP = 8 spores)
                        var nextComponent = existingComponents.Count > 0 ? existingComponents[0] : null;
                        var zoneIndex = i - hideZoneAfterIndex; // 0-based zone index

                        zoneOcclusion.SetZoneOcclusion(nextComponent, zoneIndex, this);

                        // Build new components list with ZoneOcclusion first, followed by all existing components
                        var newComponents = new List<ActivityComponent> { zoneOcclusion };
                        newComponents.AddRange(existingComponents);

                        // Set the components list
                        activityRefCopy.InitializeComponents(newComponents);

                        Debug.Log($"Added ZoneOcclusion to {activityRef.name} (index {i})");
                    }

                    var instance = new ActivityInstance(networkRun, activityRefCopy);
                    instances.Add(instance);
                }
            }
        }

        return instances;
    }

    /// <summary>
    /// Calculate required amount using RuneScape XP formula
    /// </summary>
    public int CalculateRequiredAmount(int roomLevel)
    {
        // RuneScape XP formula: calculate total XP required for this level
        // Then divide by 10 and floor to get the requirement (first level = 8, matching "8xp floor")
        float totalXP = 0f;

        for (int level = 1; level < roomLevel; level++)
        {
            totalXP += Mathf.Floor(level + 300f * Mathf.Pow(2f, level / 7f));
        }

        totalXP = Mathf.Floor(totalXP / 4f);

        // Divide by 10 and floor to get the requirement amount
        int requirement = Mathf.FloorToInt(totalXP / 10f);

        // Ensure minimum of 1
        return Mathf.Max(1, requirement);
    }

    /// <summary>
    /// Get the cost for a specific zone index, using smooth interpolation between overrides
    /// </summary>
    public int GetZoneCost(int zoneIndex)
    {
        // If no overrides, use formula with global scale
        if (zoneCostOverrides == null || zoneCostOverrides.Count == 0)
        {
            var roomLevel = zoneIndex + 2;
            var baseCost = CalculateRequiredAmount(roomLevel);
            return Mathf.CeilToInt(baseCost * zoneRevealCostScale);
        }

        // Sort overrides by zone index for interpolation
        var sortedOverrides = zoneCostOverrides.OrderBy(o => o.zoneIndex).ToList();

        // Check if this zone has an explicit override with cost >= 0
        var exactOverride = sortedOverrides.FirstOrDefault(o => o.zoneIndex == zoneIndex);
        if (exactOverride != null && exactOverride.cost >= 0)
        {
            return exactOverride.cost;
        }

        // Find the two closest override points to interpolate between
        ZoneCostOverride lowerBound = null;
        ZoneCostOverride upperBound = null;

        foreach (var o in sortedOverrides)
        {
            if (o.zoneIndex < zoneIndex)
            {
                lowerBound = o;
            }
            else if (o.zoneIndex > zoneIndex && upperBound == null)
            {
                upperBound = o;
                break;
            }
        }

        // Case 1: Before the first override - scale based on first override
        if (lowerBound == null && upperBound != null)
        {
            var roomLevel = zoneIndex + 2;
            var baseCost = CalculateRequiredAmount(roomLevel);
            var upperRoomLevel = upperBound.zoneIndex + 2;
            var upperBaseCost = CalculateRequiredAmount(upperRoomLevel);

            // Calculate scale factor from the upper bound
            var scaleFactor = (float)upperBound.cost / upperBaseCost;
            return Mathf.CeilToInt(baseCost * scaleFactor);
        }

        // Case 2: After the last override - scale based on last override
        if (lowerBound != null && upperBound == null)
        {
            var roomLevel = zoneIndex + 2;
            var baseCost = CalculateRequiredAmount(roomLevel);
            var lowerRoomLevel = lowerBound.zoneIndex + 2;
            var lowerBaseCost = CalculateRequiredAmount(lowerRoomLevel);

            // Calculate scale factor from the lower bound
            var scaleFactor = (float)lowerBound.cost / lowerBaseCost;
            return Mathf.CeilToInt(baseCost * scaleFactor);
        }

        // Case 3: Between two overrides - interpolate smoothly
        if (lowerBound != null && upperBound != null)
        {
            // Calculate base costs for interpolation reference points
            var currentRoomLevel = zoneIndex + 2;
            var currentBaseCost = CalculateRequiredAmount(currentRoomLevel);

            var lowerRoomLevel = lowerBound.zoneIndex + 2;
            var lowerBaseCost = CalculateRequiredAmount(lowerRoomLevel);

            var upperRoomLevel = upperBound.zoneIndex + 2;
            var upperBaseCost = CalculateRequiredAmount(upperRoomLevel);

            // Calculate how far we are between the two bounds (in base cost space)
            var progressInBaseCost = (float)(currentBaseCost - lowerBaseCost) / (upperBaseCost - lowerBaseCost);
            progressInBaseCost = Mathf.Clamp01(progressInBaseCost);

            // Interpolate between the override values
            var interpolatedCost = Mathf.Lerp(lowerBound.cost, upperBound.cost, progressInBaseCost);
            return Mathf.CeilToInt(interpolatedCost);
        }

        // Fallback to formula with global scale (shouldn't reach here)
        var fallbackRoomLevel = zoneIndex + 2;
        var fallbackBaseCost = CalculateRequiredAmount(fallbackRoomLevel);
        return Mathf.CeilToInt(fallbackBaseCost * zoneRevealCostScale);
    }

    /// <summary>
    /// Get additional resource cost for a zone (if any)
    /// </summary>
    public ResourceCost GetAdditionalResourceCost(int zoneIndex)
    {
        if (zoneCostOverrides == null) return null;

        var exactOverride = zoneCostOverrides.FirstOrDefault(o => o.zoneIndex == zoneIndex);
        return exactOverride?.resourceCost;
    }

    public ResourceCondition CreateResourceConditionForDoor(NetworkRun networkRun)
    {
        // Always use spores from settings
        if (sporesItem != null)
        {
            var resourceCondition = ScriptableObject.CreateInstance<ResourceCondition>();

            // Scale requirement based on number of rooms visited using RuneScape XP formula
            int requiredAmount = CalculateRequiredAmount(networkRun.VisitedRooms.Count + 2);

            // Initialize with item and amount
            resourceCondition.Initialize(sporesItem, requiredAmount);

            Debug.Log($"Created ResourceCondition requiring {requiredAmount}x {sporesItem.DisplayName}");
            return resourceCondition;
        }

        return null;
    }
}
