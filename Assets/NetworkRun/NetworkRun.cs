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
    [SerializeField] private List<UnitInstance> party;

    public Inventory Inventory => inventory;
    public RoomInstance CurrentRoom => currentRoom;
    public List<RoomInstance> VisitedRooms => visitedRooms;
    public List<UnitInstance> Party => party;

    public NetworkRun(List<DoorCondition> doorConditions, List<ActivityReference> activities, List<UnitInstance> partyUnits)
    {
        inventory = new Inventory();
        this.doorConditions = doorConditions ?? new List<DoorCondition>();
        this.activities = activities ?? new List<ActivityReference>();
        this.party = partyUnits ?? new List<UnitInstance>();
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
        // Pick 1 resource-producing activity (has ResourceUpdateComponent, no InspectComponent)
        ActivityReference activityRef = null;
        var resourceActivities = new List<ActivityReference>();

        if (activities != null && activities.Count > 0)
        {
            foreach (var activity in activities)
            {
                if (activity == null || activity.Components == null) continue;

                bool hasInspectComponent = false;
                bool hasResourceUpdateComponent = false;

                foreach (var component in activity.Components)
                {
                    if (component is InspectComponent)
                    {
                        hasInspectComponent = true;
                    }
                    else if (component is ResourceUpdateComponent)
                    {
                        hasResourceUpdateComponent = true;
                    }
                }

                // Only add activities with ResourceUpdateComponent and no InspectComponent
                if (!hasInspectComponent && hasResourceUpdateComponent)
                {
                    resourceActivities.Add(activity);
                }
            }

            if (resourceActivities.Count > 0)
            {
                activityRef = resourceActivities[UnityEngine.Random.Range(0, resourceActivities.Count)];
            }
        }

        // Create a runtime copy of the activity reference with copied components
        ActivityInstance activityInstance = null;
        if (activityRef != null)
        {
            var activityRefCopy = ScriptableObject.Instantiate(activityRef);
            activityRefCopy.name = activityRef.name;

            // Copy all components so each room has independent state
            if (activityRefCopy.Components != null && activityRefCopy.Components.Count > 0)
            {
                var copiedComponents = new List<ActivityComponent>();
                foreach (var component in activityRefCopy.Components)
                {
                    if (component != null)
                    {
                        var componentCopy = ScriptableObject.Instantiate(component);
                        componentCopy.name = component.name;
                        copiedComponents.Add(componentCopy);
                    }
                }

                // Replace the components list with the copied ones
                var componentsField = typeof(ActivityReference).GetField("components",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                componentsField?.SetValue(activityRefCopy, copiedComponents);
            }

            activityInstance = new ActivityInstance(activityRefCopy);

            // Initialize all components (iterate over a copy to avoid modification during enumeration)
            if (activityRefCopy.Components != null)
            {
                var componentsCopy = new List<ActivityComponent>(activityRefCopy.Components);
                foreach (var component in componentsCopy)
                {
                    if (component != null)
                    {
                        component.Initialize(this, activityInstance);
                    }
                }
            }
        }
        else
        {
            activityInstance = new ActivityInstance((ActivityReference)null);
        }

        // Find 1 door activity with InspectComponent
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

        var activityInstances = new List<ActivityInstance>();

        // Create doors list with no initial conditions - they will be assigned on inspect completion
        var doors = new List<Door> {
            new Door
            {
                isLocked = true,
                conditions = new List<DoorCondition>(),
                nextRoom = null
            }
        };

        // Create an unlock activity for each door (skip inspect, go straight to unlock)
        if (inspectActivityRef != null)
        {
            foreach (var door in doors)
            {
                // Create ResourceCondition for the door based on the resource activity in this room
                var resourceCondition = CreateResourceConditionForDoor(activityInstance);

                if (resourceCondition != null)
                {
                    // Add the resource condition to the door
                    if (door.conditions == null)
                    {
                        door.conditions = new System.Collections.Generic.List<DoorCondition>();
                    }
                    door.conditions.Add(resourceCondition);

                    // Create a runtime copy of the activity reference
                    var unlockRefCopy = ScriptableObject.Instantiate(inspectActivityRef);
                    unlockRefCopy.name = inspectActivityRef.name;

                    // Create UnlockComponent directly (skip InspectComponent)
                    var unlockComponent = ScriptableObject.CreateInstance<UnlockComponent>();
                    unlockComponent.name = "UnlockComponent";
                    unlockComponent.SetDoorAndCondition(door, resourceCondition);

                    // Set the components list to just the unlock component
                    var copiedComponents = new List<ActivityComponent> { unlockComponent };
                    var componentsField = typeof(ActivityReference).GetField("components",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    componentsField?.SetValue(unlockRefCopy, copiedComponents);

                    var unlockInstance = new ActivityInstance(unlockRefCopy);

                    // Initialize the unlock component
                    unlockComponent.Initialize(this, unlockInstance);

                    activityInstances.Add(unlockInstance);
                    Debug.Log($"Added unlock activity for door: {inspectActivityRef.name}");
                }
            }
        }

        // Add resource activity after door activities
        if (activityInstance != null)
        {
            activityInstances.Add(activityInstance);
        }

        return new RoomInstance(new RoomData
        {
            id = System.Guid.NewGuid().ToString(),
            name = "Generated Room",
            doors = doors,
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

    private List<ItemTemplate> GetAvailableItemsFromActivities()
    {
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
        return availableItems;
    }

    private List<ItemTemplate> GetItemsFromRoomActivities()
    {
        var roomItems = new List<ItemTemplate>();
        if (currentRoom?.Data?.activities != null)
        {
            foreach (var activity in currentRoom.Data.activities)
            {
                if (activity?.Template?.Components == null) continue;

                foreach (var component in activity.Template.Components)
                {
                    if (component is ResourceUpdateComponent resourceComponent &&
                        resourceComponent.ItemTemplate != null)
                    {
                        if (!roomItems.Contains(resourceComponent.ItemTemplate))
                        {
                            roomItems.Add(resourceComponent.ItemTemplate);
                        }
                    }
                }
            }
        }
        return roomItems;
    }

    public ResourceCondition CreateResourceConditionForDoor(ActivityInstance roomActivityInstance = null)
    {
        ItemTemplate selectedItem = null;

        // First, try to get item from the provided activity instance (the resource activity for this room)
        if (roomActivityInstance?.Template?.Components != null)
        {
            foreach (var component in roomActivityInstance.Template.Components)
            {
                if (component is ResourceUpdateComponent resourceComponent && resourceComponent.ItemTemplate != null)
                {
                    selectedItem = resourceComponent.ItemTemplate;
                    break;
                }
            }
        }

        // Fallback: try to get items from the current room's activities
        if (selectedItem == null)
        {
            var roomItems = GetItemsFromRoomActivities();
            selectedItem = roomItems.Count > 0
                ? roomItems[UnityEngine.Random.Range(0, roomItems.Count)]
                : null;
        }

        // Final fallback: any available item from all activities
        if (selectedItem == null)
        {
            var availableItems = GetAvailableItemsFromActivities();
            if (availableItems.Count > 0)
            {
                selectedItem = availableItems[UnityEngine.Random.Range(0, availableItems.Count)];
            }
        }

        if (selectedItem != null)
        {
            var resourceCondition = ScriptableObject.CreateInstance<ResourceCondition>();

            // Use reflection to set private fields
            var requiredItemField = typeof(ResourceCondition).GetField("requiredItem",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var requiredAmountField = typeof(ResourceCondition).GetField("requiredAmount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            requiredItemField?.SetValue(resourceCondition, selectedItem);

            // Scale requirement based on number of rooms visited using RuneScape XP formula
            int requiredAmount = CalculateRequiredAmount(visitedRooms.Count + 2);
            requiredAmountField?.SetValue(resourceCondition, requiredAmount);

            Debug.Log($"Created ResourceCondition requiring {requiredAmount}x {selectedItem.DisplayName}");
            return resourceCondition;
        }

        return null;
    }

}
