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
    [SerializeField] private int baseFishRewardAmount = 5;
    [SerializeField] private ItemTemplate sporesReward;
    [SerializeField] private int baseSporesRewardAmount = 5;

    private int scaledFishRewardAmount;
    private int scaledSporesRewardAmount;

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
        assignedDoor = door;
        resourceCondition = condition;
        currentResourceCount = 0;
    }

    protected override void OnInitialize()
    {
        unitProgress.Clear();

        // Calculate scaled reward amounts based on room level using RuneScape formula
        int roomLevel = networkRun?.VisitedRooms?.Count ?? 1;
        scaledFishRewardAmount = CalculateRewardAmount(roomLevel, baseFishRewardAmount);
        scaledSporesRewardAmount = CalculateRewardAmount(roomLevel, baseSporesRewardAmount);
    }

    private int CalculateRewardAmount(int roomLevel, int baseAmount)
    {
        // RuneScape XP formula: calculate total XP required for this level
        // Then divide by 10 and floor to get the scaled multiplier
        float totalXP = 0f;

        for (int level = 1; level < roomLevel; level++)
        {
            totalXP += Mathf.Floor(level + 300f * Mathf.Pow(2f, level / 7f));
        }

        totalXP = Mathf.Floor(totalXP / 4f);

        // Divide by 10 and floor to get the multiplier
        int multiplier = Mathf.Max(1, Mathf.FloorToInt(totalXP / 10f));

        // Apply multiplier to base amount
        return baseAmount * multiplier;
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
                Debug.Log($"[UnlockComponent] fishReward: {(fishReward != null ? fishReward.DisplayName : "NULL")}, amount: {scaledFishRewardAmount}");
                if (fishReward != null && scaledFishRewardAmount > 0)
                {
                    var beforeCount = networkRun.Inventory.GetItemCount(fishReward);
                    Debug.Log($"[UnlockComponent] Fish in inventory BEFORE: {beforeCount}");

                    for (int i = 0; i < scaledFishRewardAmount; i++)
                    {
                        networkRun.Inventory.AddItem(fishReward);
                        Debug.Log($"[UnlockComponent] Added fish item {i + 1}/{scaledFishRewardAmount}");
                    }

                    var afterCount = networkRun.Inventory.GetItemCount(fishReward);
                    Debug.Log($"[UnlockComponent] Fish in inventory AFTER: {afterCount}");
                    Debug.Log($"✓ Reward granted: {scaledFishRewardAmount}x {fishReward.DisplayName} (verified: {afterCount - beforeCount} added)");
                }
                else
                {
                    Debug.LogWarning($"[UnlockComponent] Fish reward NOT granted - null or zero amount");
                }

                Debug.Log($"[UnlockComponent] sporesReward: {(sporesReward != null ? sporesReward.DisplayName : "NULL")}, amount: {scaledSporesRewardAmount}");
                if (sporesReward != null && scaledSporesRewardAmount > 0)
                {
                    var beforeCount = networkRun.Inventory.GetItemCount(sporesReward);
                    Debug.Log($"[UnlockComponent] Spores in inventory BEFORE: {beforeCount}");

                    for (int i = 0; i < scaledSporesRewardAmount; i++)
                    {
                        networkRun.Inventory.AddItem(sporesReward);
                        Debug.Log($"[UnlockComponent] Added spores item {i + 1}/{scaledSporesRewardAmount}");
                    }

                    var afterCount = networkRun.Inventory.GetItemCount(sporesReward);
                    Debug.Log($"[UnlockComponent] Spores in inventory AFTER: {afterCount}");
                    Debug.Log($"✓ Reward granted: {scaledSporesRewardAmount}x {sporesReward.DisplayName} (verified: {afterCount - beforeCount} added)");
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
