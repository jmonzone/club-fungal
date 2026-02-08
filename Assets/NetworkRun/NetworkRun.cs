using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class NetworkRun
{
    private RoomInstance currentRoom;
    private Inventory inventory;
    [SerializeField] private List<DoorCondition> doorConditions;
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
    public UnlockComponent UnlockComponentTemplate => unlockComponentTemplate;

    public NetworkRun(List<DoorCondition> doorConditions, List<UnitInstance> partyUnits, UnlockComponent unlockTemplate, NetworkRunSettings settings = null)
    {
        inventory = new Inventory();
        this.doorConditions = doorConditions ?? new List<DoorCondition>();
        this.party = partyUnits ?? new List<UnitInstance>();
        this.unlockComponentTemplate = unlockTemplate;
        this.settings = settings;
        visitedRooms = new List<RoomInstance>();
        currentRoom = CreateNewRoomInstance();
        visitedRooms.Add(currentRoom);
        lastRealTime = UnityEngine.Time.realtimeSinceStartup;

        // Add all party units to rest activity if it exists
        MovePartyToRestActivity();
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

        // Add all party units to rest activity in the new room
        MovePartyToRestActivity();
    }

    private void MovePartyToRestActivity()
    {
        if (currentRoom?.Data?.activities == null || party == null) return;

        ActivityInstance restActivity = null;
        foreach (var activity in currentRoom.Data.activities)
        {
            if (activity?.Template == settings?.restActivity)
            {
                restActivity = activity;
                break;
            }
        }

        if (restActivity != null)
        {
            foreach (var unit in party)
            {
                if (unit != null)
                {
                    restActivity.AddUnit(unit, currentRoom.Data.activities);
                }
            }
        }
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
        if (settings == null) return null;

        var activityInstances = new List<ActivityInstance>();

        // Add rest activity if configured
        var restActivity = settings.CreateRestActivity(this);
        if (restActivity != null)
        {
            activityInstances.Add(restActivity);
        }

        // Add resource activities
        var resourceActivities = settings.CreateResourceActivities(this);
        activityInstances.AddRange(resourceActivities);

        // Create doors
        var doors = new List<Door> {
            new Door
            {
                isLocked = true,
                conditions = new List<DoorCondition>(),
                nextRoom = null
            }
        };

        // Add door/unlock activities
        var doorActivities = settings.CreateDoorActivities(this, doors);
        activityInstances.AddRange(doorActivities);

        return new RoomInstance(doors, activityInstances);
    }

}
