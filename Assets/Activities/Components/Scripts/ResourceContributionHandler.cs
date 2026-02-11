using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles resource contribution tracking for activities that require resources to unlock/trigger functionality.
/// Used by both ZoneOcclusion and SummonUnitComponent.
/// </summary>
[System.Serializable]
public class ResourceContributionHandler
{
    [SerializeField] private int currentResourceCount;
    [SerializeField] private int additionalResourceCount;
    [SerializeField] private int cachedRequiredAmount;
    [SerializeField] private int contextIndex = -1; // Zone index or other context identifier
    [SerializeField] private NetworkRunSettings settings;

    private Dictionary<UnitInstance, float> unitProgress = new Dictionary<UnitInstance, float>();

    [Header("Collection Settings")]
    [SerializeField] private float updateInterval = 5f;
    [SerializeField] private int itemsPerUpdate = 1;

    [Header("Resource Requirements")]
    [SerializeField] private ItemTemplate requiredItem;
    [SerializeField] private ResourceCost additionalResourceCost;

    public int CurrentResourceCount => currentResourceCount;
    public int AdditionalResourceCount => additionalResourceCount;
    public ItemTemplate RequiredItem => requiredItem;
    public ResourceCost AdditionalResourceCost => additionalResourceCost;
    public float UpdateInterval => updateInterval;
    public int ItemsPerUpdate => itemsPerUpdate;

    public int RequiredAmount
    {
        get
        {
            if (settings != null && contextIndex >= 0)
            {
                return settings.GetZoneCost(contextIndex);
            }
            return cachedRequiredAmount;
        }
    }

    public float GetUnitProgress(UnitInstance unit)
    {
        return unitProgress.ContainsKey(unit) ? unitProgress[unit] : 0f;
    }

    /// <summary>
    /// Initialize the contribution handler with settings and context
    /// </summary>
    public void Initialize(int contextIndex, NetworkRunSettings settings)
    {
        currentResourceCount = 0;
        additionalResourceCount = 0;
        this.contextIndex = contextIndex;
        this.settings = settings;
        this.cachedRequiredAmount = settings?.GetZoneCost(contextIndex) ?? 0;
        this.additionalResourceCost = settings?.GetAdditionalResourceCost(contextIndex);
        this.requiredItem = settings?.sporesItem; // Set the primary resource item
        unitProgress.Clear();
    }

    /// <summary>
    /// Reset all progress
    /// </summary>
    public void Reset()
    {
        unitProgress.Clear();
        currentResourceCount = 0;
        additionalResourceCount = 0;
    }

    /// <summary>
    /// Check if all resource requirements are satisfied
    /// </summary>
    public bool RequirementsMet()
    {
        var primarySatisfied = currentResourceCount >= RequiredAmount;
        var additionalSatisfied = additionalResourceCost == null || additionalResourceCount >= additionalResourceCost.Amount;
        return primarySatisfied && additionalSatisfied;
    }

    /// <summary>
    /// Manually contribute resources from a unit's inventory
    /// </summary>
    public void ContributeFromUnit(UnitInstance unit)
    {
        if (unit?.Inventory == null) return;

        // Contribute primary resource
        if (requiredItem != null)
        {
            var unitItemCount = unit.Inventory.GetItemCount(requiredItem);
            var remainingNeeded = RequiredAmount - currentResourceCount;
            var amountToContribute = Mathf.Min(unitItemCount, remainingNeeded);

            if (amountToContribute > 0)
            {
                for (int i = 0; i < amountToContribute; i++)
                {
                    unit.Inventory.RemoveItem(requiredItem);
                }
                currentResourceCount += amountToContribute;
                Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {requiredItem.DisplayName}. Progress: {currentResourceCount}/{RequiredAmount}");
            }
        }

        // Contribute additional resource (if any)
        if (additionalResourceCost != null && additionalResourceCost.Item != null)
        {
            var unitItemCount = unit.Inventory.GetItemCount(additionalResourceCost.Item);
            var remainingNeeded = additionalResourceCost.Amount - additionalResourceCount;
            var amountToContribute = Mathf.Min(unitItemCount, remainingNeeded);

            if (amountToContribute > 0)
            {
                for (int i = 0; i < amountToContribute; i++)
                {
                    unit.Inventory.RemoveItem(additionalResourceCost.Item);
                }
                additionalResourceCount += amountToContribute;
                Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {additionalResourceCost.Item.DisplayName}. Progress: {additionalResourceCount}/{additionalResourceCost.Amount}");
            }
        }
    }

    /// <summary>
    /// Manually contribute resources from global inventory
    /// </summary>
    public int ContributeFromGlobalInventory(Inventory globalInventory)
    {
        if (globalInventory == null) return 0;

        int totalContributed = 0;

        // Contribute primary resource
        if (requiredItem != null)
        {
            var globalItemCount = globalInventory.GetItemCount(requiredItem);
            var remainingNeeded = RequiredAmount - currentResourceCount;
            var amountToContribute = Mathf.Min(globalItemCount, remainingNeeded);

            if (amountToContribute > 0)
            {
                for (int i = 0; i < amountToContribute; i++)
                {
                    globalInventory.RemoveItem(requiredItem);
                }
                currentResourceCount += amountToContribute;
                totalContributed += amountToContribute;
                Debug.Log($"Contributed {amountToContribute}x {requiredItem.DisplayName} from global inventory. Progress: {currentResourceCount}/{RequiredAmount}");
            }
        }

        // Contribute additional resource (if any)
        if (additionalResourceCost != null && additionalResourceCost.Item != null)
        {
            var globalItemCount = globalInventory.GetItemCount(additionalResourceCost.Item);
            var remainingNeeded = additionalResourceCost.Amount - additionalResourceCount;
            var amountToContribute = Mathf.Min(globalItemCount, remainingNeeded);

            if (amountToContribute > 0)
            {
                for (int i = 0; i < amountToContribute; i++)
                {
                    globalInventory.RemoveItem(additionalResourceCost.Item);
                }
                additionalResourceCount += amountToContribute;
                totalContributed += amountToContribute;
                Debug.Log($"Contributed {amountToContribute}x {additionalResourceCost.Item.DisplayName} from global inventory. Progress: {additionalResourceCount}/{additionalResourceCost.Amount}");
            }
        }

        return totalContributed;
    }

    /// <summary>
    /// Process automatic contributions from units during update
    /// </summary>
    public bool ProcessAutomaticContributions(NetworkRun networkRun, List<UnitInstance> units)
    {
        if (units == null) return false;

        bool anyContributed = false;

        foreach (var unit in units)
        {
            if (unit?.Inventory == null) continue;

            // Initialize progress for new units
            if (!unitProgress.ContainsKey(unit))
            {
                unitProgress[unit] = 0f;
            }

            // Update progress
            var deltaTime = networkRun.Settings.speedMultiplier * Time.deltaTime;
            unitProgress[unit] += deltaTime;

            // Check if ready to contribute
            if (unitProgress[unit] >= updateInterval)
            {
                bool contributed = false;

                // Contribute primary resource
                if (requiredItem != null && currentResourceCount < RequiredAmount)
                {
                    var unitItemCount = unit.Inventory.GetItemCount(requiredItem);
                    var remainingNeeded = RequiredAmount - currentResourceCount;
                    var amountToContribute = Mathf.Min(itemsPerUpdate, unitItemCount, remainingNeeded);

                    if (amountToContribute > 0)
                    {
                        for (int i = 0; i < amountToContribute; i++)
                        {
                            unit.Inventory.RemoveItem(requiredItem);
                        }
                        currentResourceCount += amountToContribute;
                        contributed = true;
                        Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {requiredItem.DisplayName}. Progress: {currentResourceCount}/{RequiredAmount}");
                    }
                }

                // Contribute additional resource (if any)
                if (additionalResourceCost != null && additionalResourceCost.Item != null && additionalResourceCount < additionalResourceCost.Amount)
                {
                    var unitItemCount = unit.Inventory.GetItemCount(additionalResourceCost.Item);
                    var remainingNeeded = additionalResourceCost.Amount - additionalResourceCount;
                    var amountToContribute = Mathf.Min(itemsPerUpdate, unitItemCount, remainingNeeded);

                    if (amountToContribute > 0)
                    {
                        for (int i = 0; i < amountToContribute; i++)
                        {
                            unit.Inventory.RemoveItem(additionalResourceCost.Item);
                        }
                        additionalResourceCount += amountToContribute;
                        contributed = true;
                        Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {additionalResourceCost.Item.DisplayName}. Progress: {additionalResourceCount}/{additionalResourceCost.Amount}");
                    }
                }

                if (contributed)
                {
                    unitProgress[unit] = 0f;
                    anyContributed = true;
                }
            }
        }

        return anyContributed;
    }
}
