using UnityEngine;

[CreateAssetMenu(fileName = "UnlockComponent", menuName = "Club Fungal/Activities/Components/Unlock")]
public class UnlockComponent : ActivityComponent
{
    private Door assignedDoor;
    private ResourceCondition resourceCondition;
    private int currentResourceCount;

    public Door AssignedDoor => assignedDoor;
    public ResourceCondition ResourceCondition => resourceCondition;
    public int CurrentResourceCount => currentResourceCount;
    public int RequiredAmount => resourceCondition?.RequiredAmount ?? 0;
    public bool IsUnlocked => currentResourceCount >= RequiredAmount;

    public void ContributeResources(int amount)
    {
        currentResourceCount += amount;
        Debug.Log($"Contributed {amount} resources. Progress: {currentResourceCount}/{RequiredAmount}");
    }

    public void ContributeFromUnit(UnitInstance unit)
    {
        if (resourceCondition?.RequiredItem != null && unit?.Inventory != null)
        {
            var unitItemCount = unit.Inventory.GetItemCount(resourceCondition.RequiredItem);
            var remainingNeeded = RequiredAmount - currentResourceCount;
            var amountToContribute = Mathf.Min(unitItemCount, remainingNeeded);

            if (amountToContribute > 0)
            {
                // Remove items from unit inventory (only what's needed)
                for (int i = 0; i < amountToContribute; i++)
                {
                    unit.Inventory.RemoveItem(resourceCondition.RequiredItem);
                }

                // Add to progress
                currentResourceCount += amountToContribute;
                Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {resourceCondition.RequiredItem.DisplayName}. Progress: {currentResourceCount}/{RequiredAmount}");
            }
        }
    }

    public void SetDoorAndCondition(Door door, ResourceCondition condition)
    {
        displayName = $"Unlock Door";
        assignedDoor = door;
        resourceCondition = condition;
        currentResourceCount = 0;
    }

    public override void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Initialize with zero resources contributed
        // (resources must be explicitly contributed by units)
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Check if we have enough resources to unlock
        if (currentResourceCount >= RequiredAmount && assignedDoor != null && assignedDoor.isLocked)
        {
            // Door stays locked until player opens it, but we track readiness
            Debug.Log($"Enough resources collected! {currentResourceCount}/{RequiredAmount} {resourceCondition.RequiredItem.DisplayName}");
        }
    }

    public void CompleteTask(NetworkRun networkRun)
    {
        // Unlock the door (resources already contributed)
        if (assignedDoor != null && currentResourceCount >= RequiredAmount)
        {
            assignedDoor.isLocked = false;
            Debug.Log($"Door unlocked! Used {currentResourceCount}x {resourceCondition.RequiredItem.DisplayName}");
        }
    }
}
