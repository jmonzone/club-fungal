using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UnlockComponent", menuName = "Club Fungal/Activities/Components/Unlock")]
public class UnlockComponent : ActivityComponent
{
    private Door assignedDoor;
    private ResourceCondition resourceCondition;
    private ResourceContributionHandler contributionHandler = new ResourceContributionHandler();

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
    public int CurrentResourceCount => contributionHandler.CurrentResourceCount;
    public int RequiredAmount => resourceCondition?.RequiredAmount ?? 0;
    public bool IsUnlocked => CurrentResourceCount >= RequiredAmount;
    public float UpdateInterval => contributionHandler.UpdateInterval;
    public int ItemsPerUpdate => contributionHandler.ItemsPerUpdate;
    public ItemTemplate RequiredItem => resourceCondition?.RequiredItem;

    public float GetUnitProgress(UnitInstance unit)
    {
        return contributionHandler.GetUnitProgress(unit);
    }

    public ResourceContributionHandler GetContributionHandler()
    {
        return contributionHandler;
    }

    public void ContributeResources(int amount)
    {
        // Manual contribution method for compatibility
        for (int i = 0; i < amount; i++)
        {
            contributionHandler.ContributeFromUnit(null); // Legacy support
        }
    }

    public void ContributeFromUnit(UnitInstance unit)
    {
        contributionHandler.ContributeFromUnit(unit);
    }

    public void SetDoorAndCondition(Door door, ResourceCondition condition)
    {
        assignedDoor = door;
        resourceCondition = condition;

        // Initialize contribution handler with the required item and amount
        if (condition?.RequiredItem != null)
        {
            contributionHandler.InitializeWithItem(condition.RequiredItem, condition.RequiredAmount);
        }
        else
        {
            contributionHandler.Reset();
        }
    }

    protected override void OnInitialize()
    {
        // Initialize contribution handler if we have resource condition
        if (resourceCondition?.RequiredItem != null)
        {
            contributionHandler.InitializeWithItem(resourceCondition.RequiredItem, resourceCondition.RequiredAmount);
        }

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
        // If already unlocked, nothing to do
        if (IsUnlocked) return;

        // Process automatic contributions from unit inventories
        contributionHandler.ProcessAutomaticContributions(networkRun, activityInstance?.Units);
    }

    public void CompleteTask(NetworkRun networkRun)
    {
        Debug.Log($"[UnlockComponent] CompleteTask called");
        Debug.Log($"[UnlockComponent] assignedDoor: {(assignedDoor != null ? "not null" : "NULL")}");
        Debug.Log($"[UnlockComponent] currentResourceCount: {CurrentResourceCount}, RequiredAmount: {RequiredAmount}");

        // Unlock the door (resources already contributed)
        if (assignedDoor != null && IsUnlocked)
        {
            assignedDoor.isLocked = false;
            Debug.Log($"Door unlocked! Used {CurrentResourceCount}x {resourceCondition.RequiredItem.DisplayName}");

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
            Debug.LogWarning($"[UnlockComponent] CompleteTask conditions NOT met - door null: {assignedDoor == null}, insufficient resources: {!IsUnlocked}");
        }
    }
}
