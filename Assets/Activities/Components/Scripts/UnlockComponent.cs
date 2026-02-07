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

    public void SetDoorAndCondition(Door door, ResourceCondition condition)
    {
        assignedDoor = door;
        resourceCondition = condition;
        currentResourceCount = 0;
    }

    public override void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Update the current resource count from inventory
        if (resourceCondition?.RequiredItem != null && networkRun?.Inventory != null)
        {
            currentResourceCount = networkRun.Inventory.GetItemCount(resourceCondition.RequiredItem);
        }
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Keep track of the number of resources collected
        if (resourceCondition?.RequiredItem != null && networkRun?.Inventory != null)
        {
            currentResourceCount = networkRun.Inventory.GetItemCount(resourceCondition.RequiredItem);

            // Check if we have enough resources to unlock
            if (currentResourceCount >= RequiredAmount && assignedDoor != null && assignedDoor.isLocked)
            {
                // Door stays locked until player opens it, but we track readiness
                Debug.Log($"Enough resources collected! {currentResourceCount}/{RequiredAmount} {resourceCondition.RequiredItem.DisplayName}");
            }
        }
    }

    public void CompleteTask(NetworkRun networkRun)
    {
        if (resourceCondition?.RequiredItem != null && networkRun?.Inventory != null)
        {
            // Remove the required resources from inventory
            for (int i = 0; i < RequiredAmount; i++)
            {
                networkRun.Inventory.RemoveItem(resourceCondition.RequiredItem);
            }

            // Unlock the door
            if (assignedDoor != null)
            {
                assignedDoor.isLocked = false;
                Debug.Log($"Door unlocked! Consumed {RequiredAmount}x {resourceCondition.RequiredItem.DisplayName}");
            }
        }
    }
}
