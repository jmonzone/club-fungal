using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnlockComponent", menuName = "Club Fungal/Activities/Components/Unlock")]
public class UnlockComponent : ActivityComponent
{
    private Door assignedDoor;
    private ResourceCondition resourceCondition;
    private int currentResourceCount;
    private Dictionary<UnitInstance, float> unitProgress = new Dictionary<UnitInstance, float>();

    [Header("Collection Settings")]
    [SerializeField] private float updateInterval = 5f;
    [SerializeField] private int itemsPerUpdate = 1;

    [Header("Rewards")]
    [SerializeField] private ItemTemplate fishReward;
    [SerializeField] private int fishRewardAmount = 5;
    [SerializeField] private ItemTemplate sporesReward;
    [SerializeField] private int sporesRewardAmount = 5;

    public Door AssignedDoor => assignedDoor;
    public ResourceCondition ResourceCondition => resourceCondition;
    public int CurrentResourceCount => currentResourceCount;
    public int RequiredAmount => resourceCondition?.RequiredAmount ?? 0;
    public bool IsUnlocked => currentResourceCount >= RequiredAmount;
    public float UpdateInterval => updateInterval;
    public int ItemsPerUpdate => itemsPerUpdate;

    public float GetUnitProgress(UnitInstance unit)
    {
        return unitProgress.ContainsKey(unit) ? unitProgress[unit] : 0f;
    }

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
        unitProgress.Clear();
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Process each unit assigned to this activity
        if (activityInstance?.Units == null || resourceCondition?.RequiredItem == null) return;
        if (currentResourceCount >= RequiredAmount) return; // Already have enough

        foreach (var unit in activityInstance.Units)
        {
            if (unit?.Inventory == null) continue;

            // Initialize progress for new units
            if (!unitProgress.ContainsKey(unit))
            {
                unitProgress[unit] = 0f;
            }

            // Check if unit has the required item
            var unitItemCount = unit.Inventory.GetItemCount(resourceCondition.RequiredItem);
            if (unitItemCount <= 0) continue;

            // Update progress
            var deltaTime = networkRun.Settings.speedMultiplier * Time.deltaTime;
            unitProgress[unit] += deltaTime;

            // Check if ready to contribute
            if (unitProgress[unit] >= updateInterval)
            {
                var remainingNeeded = RequiredAmount - currentResourceCount;
                var amountToContribute = Mathf.Min(itemsPerUpdate, unitItemCount, remainingNeeded);

                if (amountToContribute > 0)
                {
                    // Remove items from unit inventory
                    for (int i = 0; i < amountToContribute; i++)
                    {
                        unit.Inventory.RemoveItem(resourceCondition.RequiredItem);
                    }

                    // Add to progress
                    currentResourceCount += amountToContribute;
                    unitProgress[unit] = 0f;

                    Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {resourceCondition.RequiredItem.DisplayName}. Progress: {currentResourceCount}/{RequiredAmount}");

                    // Check if we've unlocked
                    if (currentResourceCount >= RequiredAmount)
                    {
                        Debug.Log($"Enough resources collected! {currentResourceCount}/{RequiredAmount} {resourceCondition.RequiredItem.DisplayName}");
                        break;
                    }
                }
            }
        }
    }

    public void CompleteTask(NetworkRun networkRun)
    {
        Debug.Log($"[UnlockComponent] CompleteTask called");
        Debug.Log($"[UnlockComponent] assignedDoor: {(assignedDoor != null ? "not null" : "NULL")}");
        Debug.Log($"[UnlockComponent] currentResourceCount: {currentResourceCount}, RequiredAmount: {RequiredAmount}");

        // Unlock the door (resources already contributed)
        if (assignedDoor != null && currentResourceCount >= RequiredAmount)
        {
            assignedDoor.isLocked = false;
            Debug.Log($"Door unlocked! Used {currentResourceCount}x {resourceCondition.RequiredItem.DisplayName}");

            // Grant rewards
            Debug.Log($"[UnlockComponent] Checking rewards - networkRun: {(networkRun != null ? "not null" : "NULL")}, Inventory: {(networkRun?.Inventory != null ? "not null" : "NULL")}");

            if (networkRun?.Inventory != null)
            {
                Debug.Log($"[UnlockComponent] fishReward: {(fishReward != null ? fishReward.DisplayName : "NULL")}, amount: {fishRewardAmount}");
                if (fishReward != null && fishRewardAmount > 0)
                {
                    var beforeCount = networkRun.Inventory.GetItemCount(fishReward);
                    Debug.Log($"[UnlockComponent] Fish in inventory BEFORE: {beforeCount}");

                    for (int i = 0; i < fishRewardAmount; i++)
                    {
                        networkRun.Inventory.AddItem(fishReward);
                        Debug.Log($"[UnlockComponent] Added fish item {i + 1}/{fishRewardAmount}");
                    }

                    var afterCount = networkRun.Inventory.GetItemCount(fishReward);
                    Debug.Log($"[UnlockComponent] Fish in inventory AFTER: {afterCount}");
                    Debug.Log($"✓ Reward granted: {fishRewardAmount}x {fishReward.DisplayName} (verified: {afterCount - beforeCount} added)");
                }
                else
                {
                    Debug.LogWarning($"[UnlockComponent] Fish reward NOT granted - null or zero amount");
                }

                Debug.Log($"[UnlockComponent] sporesReward: {(sporesReward != null ? sporesReward.DisplayName : "NULL")}, amount: {sporesRewardAmount}");
                if (sporesReward != null && sporesRewardAmount > 0)
                {
                    var beforeCount = networkRun.Inventory.GetItemCount(sporesReward);
                    Debug.Log($"[UnlockComponent] Spores in inventory BEFORE: {beforeCount}");

                    for (int i = 0; i < sporesRewardAmount; i++)
                    {
                        networkRun.Inventory.AddItem(sporesReward);
                        Debug.Log($"[UnlockComponent] Added spores item {i + 1}/{sporesRewardAmount}");
                    }

                    var afterCount = networkRun.Inventory.GetItemCount(sporesReward);
                    Debug.Log($"[UnlockComponent] Spores in inventory AFTER: {afterCount}");
                    Debug.Log($"✓ Reward granted: {sporesRewardAmount}x {sporesReward.DisplayName} (verified: {afterCount - beforeCount} added)");
                }
                else
                {
                    Debug.LogWarning($"[UnlockComponent] Spores reward NOT granted - null or zero amount");
                }

                // Final inventory state
                Debug.Log($"[UnlockComponent] FINAL INVENTORY STATE: Fish={networkRun.Inventory.GetItemCount(fishReward)}, Spores={networkRun.Inventory.GetItemCount(sporesReward)}");
            }
            else
            {
                Debug.LogError($"[UnlockComponent] Cannot grant rewards - NetworkRun or Inventory is NULL!");
            }
        }
        else
        {
            Debug.LogWarning($"[UnlockComponent] CompleteTask conditions NOT met - door null: {assignedDoor == null}, insufficient resources: {currentResourceCount < RequiredAmount}");
        }
    }
}
