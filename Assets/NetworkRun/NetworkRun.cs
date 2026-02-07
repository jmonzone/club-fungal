using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NetworkRun
{
    private RoomInstance currentRoom;
    private Inventory inventory;
    [SerializeField] private List<DoorCondition> doorConditions;
    [SerializeField] private List<ActivityReference> activities;
    [SerializeField] private List<RoomInstance> visitedRooms;

    public Inventory Inventory => inventory;
    public RoomInstance CurrentRoom => currentRoom;
    public List<RoomInstance> VisitedRooms => visitedRooms;

    public NetworkRun(List<DoorCondition> doorConditions, List<ActivityReference> activities)
    {
        inventory = new Inventory();
        this.doorConditions = doorConditions ?? new List<DoorCondition>();
        this.activities = activities ?? new List<ActivityReference>();
        visitedRooms = new List<RoomInstance>();
        currentRoom = CreateNewRoomInstance();
        visitedRooms.Add(currentRoom);
    }

    public void SetInventory(Inventory loadedInventory)
    {
        inventory = loadedInventory;
    }

    public void SetCurrentRoom(RoomInstance roomInstance)
    {
        currentRoom = roomInstance;
    }

    public void TransitionToRoom(RoomInstance roomInstance)
    {
        if (roomInstance == null) return;

        currentRoom = roomInstance;
        if (!visitedRooms.Contains(roomInstance))
        {
            visitedRooms.Add(roomInstance);
        }
        Debug.Log($"Transitioned to room: {currentRoom.Data.name} (Total rooms visited: {visitedRooms.Count})");
    }

    public bool OpenDoorAndTransition(Door door)
    {
        if (door == null) return false;

        var nextRoomInstance = door.Open(inventory);

        if (nextRoomInstance == null && doorConditions != null && doorConditions.Count > 0)
        {
            nextRoomInstance = CreateNewRoomInstance();
        }

        if (nextRoomInstance != null)
        {
            TransitionToRoom(nextRoomInstance);
            return true;
        }

        return false;
    }

    private RoomInstance CreateNewRoomInstance()
    {
        // Primarily create a new ResourceCondition with a random resource and count of 10
        DoorCondition doorCondition = null;

        // Collect all available ItemTemplates from activities
        var availableItems = new List<ItemTemplate>();
        if (activities != null)
        {
            foreach (var activity in activities)
            {
                if (activity == null || activity.Components == null) continue;

                foreach (var component in activity.Components)
                {
                    if (component is ResourceUpdateComponent resourceComponent &&
                        resourceComponent.ItemTemplate != null)
                    {
                        if (!availableItems.Contains(resourceComponent.ItemTemplate))
                        {
                            availableItems.Add(resourceComponent.ItemTemplate);
                        }
                    }
                }
            }
        }

        // Create a new ResourceCondition with a random ItemTemplate
        if (availableItems.Count > 0)
        {
            var randomItem = availableItems[UnityEngine.Random.Range(0, availableItems.Count)];
            var resourceCondition = ScriptableObject.CreateInstance<ResourceCondition>();

            // Use reflection to set private fields
            var requiredItemField = typeof(ResourceCondition).GetField("requiredItem",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var requiredAmountField = typeof(ResourceCondition).GetField("requiredAmount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            requiredItemField?.SetValue(resourceCondition, randomItem);

            // Scale requirement based on number of rooms visited using RuneScape XP formula
            int requiredAmount = CalculateRequiredAmount(visitedRooms.Count + 2);
            requiredAmountField?.SetValue(resourceCondition, requiredAmount);

            doorCondition = resourceCondition;
            Debug.Log($"Created new ResourceCondition requiring {requiredAmount}x {randomItem.DisplayName} (Room {visitedRooms.Count + 1})");
        }

        // Fallback to selecting from predefined doorConditions
        if (doorCondition == null && doorConditions != null && doorConditions.Count > 0)
        {
            doorCondition = doorConditions[UnityEngine.Random.Range(0, doorConditions.Count)];
            Debug.Log($"Using fallback door condition: {doorCondition.GetDescription()}");
        }

        Debug.Log($"Door is locked. Attempting to satisfy condition: {doorCondition?.GetDescription() ?? "None"}");
        // Try to find an activity that provides the resource required by the door condition
        ActivityReference activityRef = null;
        if (doorCondition is ResourceCondition _resourceCondition && _resourceCondition.RequiredItem != null)
        {
            activityRef = FindActivityProvidingItem(_resourceCondition.RequiredItem);
        }

        // Fallback to random activity if no matching activity found
        if (activityRef == null && activities != null && activities.Count > 0)
        {
            activityRef = activities[UnityEngine.Random.Range(0, activities.Count)];
        }

        var activityInstance = new ActivityInstance(activityRef);

        // Find an activity with InspectComponent for door inspection
        ActivityReference inspectActivityRef = null;
        if (activities != null && activities.Count > 0)
        {
            foreach (var activity in activities)
            {
                if (activity == null || activity.Components == null) continue;

                foreach (var component in activity.Components)
                {
                    if (component is InspectComponent)
                    {
                        inspectActivityRef = activity;
                        break;
                    }
                }

                if (inspectActivityRef != null) break;
            }
        }

        var activityInstances = new List<ActivityInstance> { activityInstance };

        if (inspectActivityRef != null)
        {
            // Create a runtime copy of the activity reference with copied components
            var inspectRefCopy = ScriptableObject.Instantiate(inspectActivityRef);

            // Copy all components so each room has independent state
            if (inspectRefCopy.Components != null && inspectRefCopy.Components.Count > 0)
            {
                var copiedComponents = new List<ActivityComponent>();
                foreach (var component in inspectRefCopy.Components)
                {
                    if (component != null)
                    {
                        var componentCopy = ScriptableObject.Instantiate(component);
                        copiedComponents.Add(componentCopy);
                    }
                }

                // Replace the components list with the copied ones
                var componentsField = typeof(ActivityReference).GetField("components",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                componentsField?.SetValue(inspectRefCopy, copiedComponents);
            }

            var inspectInstance = new ActivityInstance(inspectRefCopy);
            activityInstances.Add(inspectInstance);
            Debug.Log($"Added inspect activity copy: {inspectActivityRef.name}");
        }

        return new RoomInstance(new RoomData
        {
            id = System.Guid.NewGuid().ToString(),
            name = "Generated Room",
            doors = new List<Door> {
                        new Door
                        {
                            isLocked = false,
                            conditions = new List<DoorCondition> { doorCondition },
                            nextRoom = null
                        }
                    },
            activities = activityInstances
        });
    }

    private ActivityReference FindActivityProvidingItem(ItemTemplate requiredItem)
    {
        if (activities == null || activities.Count == 0 || requiredItem == null)
            return null;

        var matchingActivities = new System.Collections.Generic.List<ActivityReference>();
        Debug.Log($"Looking for activities that provide required item: {requiredItem.DisplayName}");
        foreach (var activity in activities)
        {
            if (activity == null || activity.Components == null) continue;

            foreach (var component in activity.Components)
            {
                if (component is ResourceUpdateComponent resourceComponent)
                {
                    if (resourceComponent.ItemTemplate != null &&
                        resourceComponent.ItemTemplate.Id == requiredItem.Id)
                    {
                        Debug.Log($"Found matching activity: {activity.name} provides required item: {requiredItem.DisplayName}");
                        matchingActivities.Add(activity);
                        break;
                    }
                }
            }
        }

        if (matchingActivities.Count > 0)
        {
            var selectedActivity = matchingActivities[UnityEngine.Random.Range(0, matchingActivities.Count)];
            Debug.Log($"Found activity providing {requiredItem.DisplayName}: {selectedActivity.name}");
            return selectedActivity;
        }

        return null;
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

}
