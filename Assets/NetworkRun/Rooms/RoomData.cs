using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

public abstract class DoorCondition : ScriptableObject
{
    public abstract bool IsMet(Inventory inventory);
    public abstract string GetDescription();
    public abstract void Satisfy(Inventory inventory);
}

[Serializable]
public class Door
{
    public bool isLocked = true;
    public List<DoorCondition> conditions = new List<DoorCondition>();
    public RoomInstance nextRoom;

    public RoomInstance Open(Inventory inventory)
    {
        if (inventory == null) return null;

        // Satisfy all conditions
        if (conditions != null)
        {
            foreach (var condition in conditions)
            {
                if (condition != null)
                {
                    condition.Satisfy(inventory);
                }
            }
        }

        // Lock the door after opening (consumed resources)
        isLocked = true;

        return nextRoom;
    }
}

[Serializable]
public class RoomData
{
    public string id;
    public string name;
    public List<Door> doors;
    public List<ActivityInstance> activities;
}
