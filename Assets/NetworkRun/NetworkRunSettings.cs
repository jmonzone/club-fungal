using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public int hideResourceComponentsAfterIndex = 0; // Only hide resource components starting from this index (0 = hide all)
    public bool enableDoorActivities = true; // Enable/disable door unlock activities
    public bool unitsCollectToGlobalInventory = false; // If true, units add collected resources directly to global inventory instead of their own
    public bool showUnitInventoryButton = true; // Show "View Data" button on unit cards
    public bool showResourceSkillLevel = false; // Show skill level and XP progress on resource unit cards
    public ActivityReference restActivity; // Rest activity
    public ActivityReference sporeActivity; // Rest activity
    public ActivityReference doorActivity; // Door/unlock activity
    public List<ActivityReference> resourceActivities; // Resource-producing activities (filtered)

    [Header("Skills")]
    [SerializeField] private List<SkillToggle> skills = new List<SkillToggle>(); // Skills that can be toggled on/off

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

    public ActivityInstance CreateRestActivity(NetworkRun networkRun)
    {
        if (restActivity == null) return null;

        var restActivityInstance = new ActivityInstance(networkRun, restActivity);
        Debug.Log($"Added rest activity: {restActivity.name}");
        return restActivityInstance;
    }

    public ActivityInstance CreateSporeActivity(NetworkRun networkRun)
    {
        if (sporeActivity == null) return null;

        var sporeActivityInstance = new ActivityInstance(networkRun, sporeActivity);
        Debug.Log($"Added spore activity: {sporeActivity.name}");
        return sporeActivityInstance;
    }

    public List<ActivityInstance> CreateDoorActivities(NetworkRun networkRun, List<Door> doors)
    {
        var doorActivities = new List<ActivityInstance>();
        if (!enableDoorActivities || doorActivity == null) return doorActivities;

        foreach (var door in doors)
        {
            var resourceCondition = CreateResourceConditionForDoor(networkRun);
            if (resourceCondition == null) continue;

            // Add the resource condition to the door
            if (door.conditions == null)
            {
                door.conditions = new List<DoorCondition>();
            }
            door.conditions.Add(resourceCondition);

            // Create a runtime copy of the activity reference
            var unlockRefCopy = Instantiate(doorActivity);
            unlockRefCopy.name = doorActivity.name;

            // Create UnlockComponent from template or create new instance
            UnlockComponent unlockComponent;
            if (networkRun.UnlockComponentTemplate != null)
            {
                unlockComponent = Instantiate(networkRun.UnlockComponentTemplate);
                unlockComponent.name = networkRun.UnlockComponentTemplate.name;
            }
            else
            {
                unlockComponent = CreateInstance<UnlockComponent>();
                unlockComponent.name = "UnlockComponent";
            }
            unlockComponent.SetDoorAndCondition(door, resourceCondition);

            // Set the components list to just the unlock component
            var copiedComponents = new List<ActivityComponent> { unlockComponent };
            unlockRefCopy.InitializeComponents(copiedComponents);

            var unlockInstance = new ActivityInstance(unlockRefCopy);
            unlockComponent.Initialize(networkRun, unlockInstance);

            doorActivities.Add(unlockInstance);
            Debug.Log($"Added unlock activity for door: {doorActivity.name}");
        }

        return doorActivities;
    }

    public List<ActivityInstance> CreateResourceActivities(NetworkRun networkRun)
    {
        if (resourceActivities == null || resourceActivities.Count == 0)
        {
            return new List<ActivityInstance>();
        }

        // Determine how many activities to create (scale with room count, respecting minimum, max at unique activity count)
        int baseCount = networkRun.VisitedRooms.Count + 1;
        int activityCount = Mathf.Max(minimumActivities, Mathf.Min(baseCount, resourceActivities.Count));
        var selectedActivities = new List<ActivityReference>();

        if (activityCount > 0)
        {
            // Shuffle indices to randomly select activities
            var indices = new List<int>();
            for (int i = 0; i < resourceActivities.Count; i++)
            {
                indices.Add(i);
            }

            for (int i = 0; i < indices.Count; i++)
            {
                var temp = indices[i];
                int randomIndex = UnityEngine.Random.Range(i, indices.Count);
                indices[i] = indices[randomIndex];
                indices[randomIndex] = temp;
            }

            // Take the first activityCount indices and sort them to maintain original order
            var selectedIndices = new List<int>();
            for (int i = 0; i < activityCount; i++)
            {
                selectedIndices.Add(indices[i]);
            }
            selectedIndices.Sort();

            // Get activities in original order
            for (int i = 0; i < selectedIndices.Count; i++)
            {
                selectedActivities.Add(resourceActivities[selectedIndices[i]]);
            }
        }

        // Create activity instances for all selected activities
        var resourceActivityInstances = new List<ActivityInstance>();
        for (int i = 0; i < selectedActivities.Count; i++)
        {
            var activityRef = selectedActivities[i];
            // Create a runtime copy of the activity reference
            var activityRefCopy = Instantiate(activityRef);
            activityRefCopy.name = activityRef.name;

            // Add ZoneOcclusion component if enabled, template exists, and index is >= threshold
            if (enableZoneOcclusion && zoneOcclusionTemplate != null && sporesItem != null && i >= hideResourceComponentsAfterIndex)
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

                // Initialize zone occlusion with the first existing component and room level
                // Use +2 offset to match door unlock formula (first room level = 2 = 83 XP = 8 spores)
                var nextComponent = existingComponents.Count > 0 ? existingComponents[0] : null;
                var roomLevel = (i - hideResourceComponentsAfterIndex) + 2;
                zoneOcclusion.SetZoneOcclusion(nextComponent, roomLevel);

                // Build new components list: ZoneOcclusion first, then existing components
                var newComponents = new List<ActivityComponent> { zoneOcclusion };
                newComponents.AddRange(existingComponents);

                // Set the components list
                activityRefCopy.InitializeComponents(newComponents);

                Debug.Log($"Added ZoneOcclusion to {activityRef.name}");
            }

            var activityInstance = new ActivityInstance(networkRun, activityRefCopy);
            if (activityInstance != null)
            {
                resourceActivityInstances.Add(activityInstance);
            }
        }

        Debug.Log($"Created {resourceActivityInstances.Count} resource activities for room {networkRun.VisitedRooms.Count + 1}");
        return resourceActivityInstances;
    }

    private int CalculateRequiredAmount(int roomLevel)
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
