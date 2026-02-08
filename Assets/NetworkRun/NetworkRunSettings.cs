using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkRunSettings", menuName = "Club Fungal/Network Run/Settings")]
public class NetworkRunSettings : ScriptableObject
{
    // Add settings fields as needed
    public int defaultPartySize = 3;
    public float updateInterval = 1.0f;
    public bool debugMode = false;
    public float speedMultiplier = 1.0f; // Simulation speed (1.0 = normal, 2.0 = 2x speed, etc.)
    public ItemTemplate sporesItem; // Item required for door unlocks
    public ActivityReference restActivity; // Rest activity
    public ActivityReference sporeActivity; // Rest activity
    public ActivityReference doorActivity; // Door/unlock activity
    public List<ActivityReference> resourceActivities; // Resource-producing activities (filtered)
    // Add more settings here

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
        if (doorActivity == null) return doorActivities;

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
            var unlockRefCopy = ScriptableObject.Instantiate(doorActivity);
            unlockRefCopy.name = doorActivity.name;

            // Create UnlockComponent from template or create new instance
            UnlockComponent unlockComponent;
            if (networkRun.UnlockComponentTemplate != null)
            {
                unlockComponent = ScriptableObject.Instantiate(networkRun.UnlockComponentTemplate);
                unlockComponent.name = networkRun.UnlockComponentTemplate.name;
            }
            else
            {
                unlockComponent = ScriptableObject.CreateInstance<UnlockComponent>();
                unlockComponent.name = "UnlockComponent";
            }
            unlockComponent.SetDoorAndCondition(door, resourceCondition);

            // Set the components list to just the unlock component
            var copiedComponents = new List<ActivityComponent> { unlockComponent };
            var componentsField = typeof(ActivityReference).GetField("components",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            componentsField?.SetValue(unlockRefCopy, copiedComponents);

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

        // Determine how many activities to create (scale with room count, max at unique activity count)
        int activityCount = Mathf.Min(networkRun.VisitedRooms.Count + 1, resourceActivities.Count);
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
        foreach (var activityRef in selectedActivities)
        {
            var activityInstance = new ActivityInstance(networkRun, activityRef);
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
