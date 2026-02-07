using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class DoorCondition : ScriptableObject
{
    public abstract bool IsMet(Inventory inventory);
    public abstract string GetDescription();
}

[CreateAssetMenu(fileName = "ResourceCondition", menuName = "Club Fungal/Network Run/Resource Condition")]
public class ResourceCondition : DoorCondition
{
    [SerializeField] private ItemTemplate requiredItem;
    [SerializeField] private int requiredAmount;

    public ItemTemplate RequiredItem => requiredItem;
    public int RequiredAmount => requiredAmount;

    public override bool IsMet(Inventory inventory)
    {
        if (requiredItem == null) return true;
        return inventory.GetItemCount(requiredItem) >= requiredAmount;
    }

    public override string GetDescription()
    {
        if (requiredItem == null) return "No requirement";
        return $"{requiredAmount}x {requiredItem.DisplayName}";
    }
}

[Serializable]
public class Door
{
    public bool isLocked = true;
    public List<DoorCondition> conditions = new List<DoorCondition>();
}

[Serializable]
public class RoomData
{
    public string id;
    public string name;
    public List<Door> doors;
    public List<ActivityInstance> activities;
}
