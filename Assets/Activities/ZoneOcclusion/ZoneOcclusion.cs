using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ZoneOcclusion", menuName = "Club Fungal/Activities/Components/ZoneOcclusion")]
public class ZoneOcclusion : ActivityComponent
{
    private ActivityComponent hiddenZoneComponent;
    [SerializeField] private int currentResourceCount;
    [SerializeField] private bool isRevealed = false;
    private Dictionary<UnitInstance, float> unitProgress = new Dictionary<UnitInstance, float>();

    [Header("Collection Settings")]
    [SerializeField] private float updateInterval = 5f;
    [SerializeField] private int itemsPerUpdate = 1;

    [Header("Occlusion Settings")]
    [SerializeField] private ItemTemplate requiredItem; // Spores by default
    [SerializeField] private Sprite occludedSprite; // Icon to show when zone is hidden
    [SerializeField] private string occludedDisplayName = "Hidden Zone"; // Name to show when zone is hidden

    public int CurrentResourceCount => currentResourceCount;

    [SerializeField] private int requiredAmount;
    public int RequiredAmount => requiredAmount;
    public bool IsRevealed => isRevealed;
    public float UpdateInterval => updateInterval;
    public int ItemsPerUpdate => itemsPerUpdate;
    public ItemTemplate RequiredItem => requiredItem;
    public Sprite OccludedSprite => occludedSprite;
    public string OccludedDisplayName => occludedDisplayName;

    public float GetUnitProgress(UnitInstance unit)
    {
        return unitProgress.ContainsKey(unit) ? unitProgress[unit] : 0f;
    }

    public void SetZoneOcclusion(ActivityComponent nextComponent, int numberOfRooms)
    {
        hiddenZoneComponent = nextComponent;
        currentResourceCount = 0;
        isRevealed = false;
        requiredAmount = CalculateRequiredAmount(numberOfRooms);

        Debug.Log($"[ZoneOcclusion] Zone hidden. Requires {RequiredAmount}x {requiredItem?.DisplayName} to reveal.");
    }

    public void ContributeFromUnit(UnitInstance unit)
    {
        if (requiredItem != null && unit?.Inventory != null && !isRevealed)
        {
            var unitItemCount = unit.Inventory.GetItemCount(requiredItem);
            var remainingNeeded = RequiredAmount - currentResourceCount;
            var amountToContribute = Mathf.Min(unitItemCount, remainingNeeded);

            if (amountToContribute > 0)
            {
                // Remove items from unit inventory (only what's needed)
                for (int i = 0; i < amountToContribute; i++)
                {
                    unit.Inventory.RemoveItem(requiredItem);
                }

                // Add to progress
                currentResourceCount += amountToContribute;
                Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {requiredItem.DisplayName}. Progress: {currentResourceCount}/{RequiredAmount}");
            }
        }
    }

    public int ContributeFromGlobalInventory(Inventory globalInventory)
    {
        if (requiredItem != null && globalInventory != null && !isRevealed)
        {
            var globalItemCount = globalInventory.GetItemCount(requiredItem);
            var remainingNeeded = RequiredAmount - currentResourceCount;
            var amountToContribute = Mathf.Min(globalItemCount, remainingNeeded);

            if (amountToContribute > 0)
            {
                // Remove items from global inventory
                for (int i = 0; i < amountToContribute; i++)
                {
                    globalInventory.RemoveItem(requiredItem);
                }

                // Add to progress
                currentResourceCount += amountToContribute;
                Debug.Log($"Contributed {amountToContribute}x {requiredItem.DisplayName} from global inventory. Progress: {currentResourceCount}/{RequiredAmount}");

                // Check if we've revealed the zone
                if (currentResourceCount >= RequiredAmount)
                {
                    RevealZone();
                }

                return amountToContribute;
            }
        }

        return 0;
    }

    public override void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        unitProgress.Clear();
    }

    private int CalculateRequiredAmount(int roomLevel)
    {
        // RuneScape XP formula: calculate total XP required for this level
        // Then divide by 10 and floor to get the requirement
        // Match door unlock formula (first room with level 2 = 83 XP = 8 spores)
        float totalXP = 0f;

        for (int level = 1; level < roomLevel; level++)
        {
            totalXP += Mathf.Floor(level + 300f * Mathf.Pow(2f, level / 7f));
        }

        totalXP = Mathf.Floor(totalXP / 4f);

        // Divide by 10 and floor to get the requirement amount
        int requirement = Mathf.FloorToInt(totalXP / 10f);

        // Ensure minimum of 1
        int result = Mathf.Max(1, requirement);
        Debug.Log($"[ZoneOcclusion] CalculateRequiredAmount: roomLevel={roomLevel}, totalXP={totalXP}, requirement={requirement}, result={result}");
        return result;
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Don't process if already revealed
        if (isRevealed) return;

        // Process each unit assigned Ro this activity
        if (activityInstance?.Units == null || requiredItem == null) return;

        foreach (var unit in activityInstance.Units)
        {
            if (unit?.Inventory == null) continue;

            // Initialize progress for new units
            if (!unitProgress.ContainsKey(unit))
            {
                unitProgress[unit] = 0f;
            }

            // Check if unit has the required item
            var unitItemCount = unit.Inventory.GetItemCount(requiredItem);
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
                        unit.Inventory.RemoveItem(requiredItem);
                    }

                    // Add to progress
                    currentResourceCount += amountToContribute;
                    unitProgress[unit] = 0f;

                    Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {requiredItem.DisplayName}. Progress: {currentResourceCount}/{RequiredAmount}");

                    // Check if we've revealed the zone
                    if (currentResourceCount >= RequiredAmount)
                    {
                        RevealZone();
                        break;
                    }
                }
            }
        }
    }

    public void RevealZone()
    {
        if (!isRevealed && currentResourceCount >= RequiredAmount)
        {
            isRevealed = true;
            Debug.Log($"[ZoneOcclusion] Zone revealed! Used {currentResourceCount}x {requiredItem?.DisplayName}");
            // The hidden zone component is now accessible (drawer will handle visibility)
        }
    }
}
