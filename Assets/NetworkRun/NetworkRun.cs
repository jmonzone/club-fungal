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
    [SerializeField] private UnlockComponent unlockComponentTemplate;
    [SerializeField] private NetworkRunSettings settings;

    private float simulationTime = 0f;
    private float lastRealTime = 0f;

    public Inventory Inventory => inventory;
    public RoomInstance CurrentRoom => currentRoom;
    public List<RoomInstance> VisitedRooms => visitedRooms;
    public List<UnitInstance> Party => party;
    public NetworkRunSettings Settings => settings;
    public float SimulationTime => simulationTime;

    public NetworkRun(List<DoorCondition> doorConditions, List<ActivityReference> activities, List<UnitInstance> partyUnits, UnlockComponent unlockTemplate, NetworkRunSettings settings = null)
    {
        inventory = new Inventory();
        this.doorConditions = doorConditions ?? new List<DoorCondition>();
        this.activities = activities ?? new List<ActivityReference>();
        this.party = partyUnits ?? new List<UnitInstance>();
        this.unlockComponentTemplate = unlockTemplate;
        this.settings = settings;
        visitedRooms = new List<RoomInstance>();
        currentRoom = CreateNewRoomInstance();
        visitedRooms.Add(currentRoom);
        lastRealTime = UnityEngine.Time.realtimeSinceStartup;
    }

    public void UpdateSimulationTime()
    {
        float currentRealTime = UnityEngine.Time.realtimeSinceStartup;
        float deltaTime = currentRealTime - lastRealTime;
        float speedMultiplier = settings?.speedMultiplier ?? 1f;
        simulationTime += deltaTime * speedMultiplier;
        lastRealTime = currentRealTime;
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

        Debug.Log($"[NetworkRun] OpenDoorAndTransition called. Inventory stacks: {inventory?.ItemStacks?.Count ?? -1}");

        var nextRoomInstance = door.Open(inventory);

        if (nextRoomInstance == null && doorConditions != null && doorConditions.Count > 0)
        {
            Debug.Log($"[NetworkRun] Creating new room instance. Inventory before: {inventory?.ItemStacks?.Count ?? -1}");
            nextRoomInstance = CreateNewRoomInstance();
            Debug.Log($"[NetworkRun] New room created. Inventory after: {inventory?.ItemStacks?.Count ?? -1}");
        }

        if (nextRoomInstance != null)
        {
            TransitionToRoom(nextRoomInstance);
            Debug.Log($"[NetworkRun] Transitioned to new room. Final inventory stacks: {inventory?.ItemStacks?.Count ?? -1}");
            return true;
        }

        return false;
    }

    private RoomInstance CreateNewRoomInstance()
    {
        // Pick resource-producing activities (has ResourceUpdateComponent, no InspectComponent)
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
        }

        // Determine how many activities to create (scale with room count, max at unique activity count)
        int activityCount = Mathf.Min(visitedRooms.Count + 1, resourceActivities.Count);
        var selectedActivities = new List<ActivityReference>();

        if (resourceActivities.Count > 0 && activityCount > 0)
        {
            // Shuffle and take the required number of unique activities
            var shuffled = new List<ActivityReference>(resourceActivities);
            for (int i = 0; i < shuffled.Count; i++)
            {
                var temp = shuffled[i];
                int randomIndex = UnityEngine.Random.Range(i, shuffled.Count);
                shuffled[i] = shuffled[randomIndex];
                shuffled[randomIndex] = temp;
            }

            for (int i = 0; i < activityCount; i++)
            {
                selectedActivities.Add(shuffled[i]);
            }
        }

        // Create activity instances for all selected activities
        var resourceActivityInstances = new List<ActivityInstance>();
        foreach (var activityRef in selectedActivities)
        {
            // Create a runtime copy of the activity reference with copied components
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

            var activityInstance = new ActivityInstance(activityRefCopy);

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

            resourceActivityInstances.Add(activityInstance);
        }

        Debug.Log($"Created {resourceActivityInstances.Count} resource activities for room {visitedRooms.Count + 1}");

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
                // Create ResourceCondition for the door based on the first resource activity in this room
                var firstActivityInstance = resourceActivityInstances.Count > 0 ? resourceActivityInstances[0] : null;
                var resourceCondition = CreateResourceConditionForDoor(firstActivityInstance);

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

                    // Create UnlockComponent from template or create new instance
                    UnlockComponent unlockComponent;
                    if (unlockComponentTemplate != null)
                    {
                        unlockComponent = ScriptableObject.Instantiate(unlockComponentTemplate);
                        unlockComponent.name = unlockComponentTemplate.name;
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

                    // Initialize the unlock component
                    unlockComponent.Initialize(this, unlockInstance);

                    activityInstances.Add(unlockInstance);
                    Debug.Log($"Added unlock activity for door: {inspectActivityRef.name}");
                }
            }
        }

        // Add resource activities after door activities
        activityInstances.AddRange(resourceActivityInstances);

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
