using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Status of a unit's contribution to a resource-based activity
/// </summary>
public enum ContributionStatus
{
    Complete,          // Requirements met or task complete
    Contributing,      // Unit has resources and is actively contributing
    NeedsResource,     // Unit lacks the required resource
    WaitingForReveal   // Requirements met but waiting for manual reveal/trigger
}

/// <summary>
/// Handles resource contribution tracking for activities that require resources to unlock/trigger functionality.
/// Used by both ZoneOcclusion and SummonUnitComponent.
/// </summary>
[System.Serializable]
public class ResourceContributionHandler
{
    [SerializeField] private int currentResourceCount;
    [SerializeField] private int additionalResourceCount;
    [SerializeField] private int currentIndex = 0; // Current level/index for scaling
    [SerializeField] private int initialIndex = 0; // Starting index to detect "first item"
    [SerializeField] private bool hasUsedFreeItem = false; // Track if free item has been claimed
    [SerializeField] private int fixedAmount = -1; // If >= 0, use this instead of scaling (-1 = use scaling)
    [SerializeField] private ResourceScalingConfig scalingConfig;
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
    public ResourceCost AdditionalResourceCost
    {
        get
        {
            // Get from scaling config if available
            if (scalingConfig != null)
            {
                return scalingConfig.GetAdditionalResourceCost(LogicalIndex);
            }
            // Fallback to stored value
            return additionalResourceCost;
        }
    }
    public float UpdateInterval => updateInterval;
    public int ItemsPerUpdate => itemsPerUpdate;

    /// <summary>
    /// Get logical 0-based index for cost overrides (0 = first item, 1 = second item, etc.)
    /// </summary>
    private int LogicalIndex => currentIndex - initialIndex;

    public int RequiredAmount
    {
        get
        {
            // Use fixed amount if set
            if (fixedAmount >= 0)
            {
                return fixedAmount;
            }

            // Check if first item is free (scaling config with firstItemFree enabled and not yet used)
            if (scalingConfig != null && scalingConfig.firstItemFree && !hasUsedFreeItem)
            {
                return 0;
            }

            // Use scaling config if available
            if (scalingConfig != null)
            {
                return scalingConfig.GetCost(LogicalIndex);
            }

            // Fallback to NetworkRunSettings zone cost if no scaling config
            if (settings != null && currentIndex >= 0)
            {
                return settings.GetZoneCost(currentIndex);
            }

            return 1; // Minimum default
        }
    }

    public float GetUnitProgress(UnitInstance unit)
    {
        return unitProgress.ContainsKey(unit) ? unitProgress[unit] : 0f;
    }

    /// <summary>
    /// Initialize the contribution handler with settings and context
    /// </summary>
    public void Initialize(int contextIndex, NetworkRunSettings settings, ResourceScalingConfig scalingConfig = null)
    {
        currentResourceCount = 0;
        additionalResourceCount = 0;
        // Add 2 to contextIndex to start at level 2 (cost 8) for first item, level 3 (cost 17) for second, etc.
        this.currentIndex = contextIndex + 2;
        // initialIndex should always be 2 (the starting formula level) for proper LogicalIndex calculation
        this.initialIndex = 2;
        this.hasUsedFreeItem = false; // Reset free item flag
        this.settings = settings;
        this.scalingConfig = scalingConfig;
        // additionalResourceCost now comes from scalingConfig dynamically
        this.additionalResourceCost = null;

        // Use spores as default if requiredItem not already set in component asset
        if (requiredItem == null)
        {
            this.requiredItem = settings?.sporesItem;
        }

        unitProgress.Clear();
    }

    /// <summary>
    /// Initialize with explicit required item and scaling config
    /// </summary>
    public void InitializeWithItem(ItemTemplate item, int startIndex = 0, ResourceScalingConfig scalingConfig = null)
    {
        currentResourceCount = 0;
        additionalResourceCount = 0;
        this.requiredItem = item;
        this.currentIndex = startIndex;
        this.initialIndex = startIndex; // Track initial value to detect "first item"
        this.scalingConfig = scalingConfig;
        this.fixedAmount = -1; // Use scaling
        this.additionalResourceCost = null;
        this.settings = null;
        unitProgress.Clear();
    }

    /// <summary>
    /// Initialize with a fixed cost amount (no scaling)
    /// </summary>
    public void InitializeWithFixedCost(ItemTemplate item, int amount)
    {
        currentResourceCount = 0;
        additionalResourceCount = 0;
        this.requiredItem = item;
        this.fixedAmount = amount;
        this.scalingConfig = null;
        this.currentIndex = 0;
        this.additionalResourceCost = null;
        this.settings = null;
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
    /// Update the current index (level) for dynamic scaling
    /// </summary>
    public void UpdateIndex(int index)
    {
        this.currentIndex = index;
        // Mark free item as used once we move away from initial state
        if (scalingConfig != null && scalingConfig.firstItemFree && !hasUsedFreeItem)
        {
            hasUsedFreeItem = true;
        }
    }

    /// <summary>
    /// Check if all resource requirements are satisfied
    /// </summary>
    public bool RequirementsMet()
    {
        var primarySatisfied = currentResourceCount >= RequiredAmount;
        var additionalCost = AdditionalResourceCost;
        var additionalSatisfied = additionalCost == null || additionalResourceCount >= additionalCost.Amount;
        return primarySatisfied && additionalSatisfied;
    }

    /// <summary>
    /// Manually contribute resources from a unit's inventory
    /// </summary>
    public void ContributeFromUnit(UnitInstance unit)
    {
        if (unit?.Inventory == null) return;

        // Prioritize primary resource if unit has it and it's not complete
        if (requiredItem != null && currentResourceCount < RequiredAmount)
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
                return; // Only contribute one resource type at a time
            }
        }

        // Contribute additional resource if primary is done or unit doesn't have primary resource
        var additionalCost = AdditionalResourceCost;
        if (additionalCost != null && additionalCost.Item != null && additionalResourceCount < additionalCost.Amount)
        {
            var unitItemCount = unit.Inventory.GetItemCount(additionalCost.Item);
            var remainingNeeded = additionalCost.Amount - additionalResourceCount;
            var amountToContribute = Mathf.Min(unitItemCount, remainingNeeded);

            if (amountToContribute > 0)
            {
                for (int i = 0; i < amountToContribute; i++)
                {
                    unit.Inventory.RemoveItem(additionalCost.Item);
                }
                additionalResourceCount += amountToContribute;
                Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {additionalCost.Item.DisplayName}. Progress: {additionalResourceCount}/{additionalCost.Amount}");
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

        // Prioritize primary resource if not complete
        if (requiredItem != null && currentResourceCount < RequiredAmount)
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
                return totalContributed; // Only contribute one resource type at a time
            }
        }

        // Contribute additional resource if primary is done or global inventory doesn't have primary resource
        var additionalCost = AdditionalResourceCost;
        if (additionalCost != null && additionalCost.Item != null && additionalResourceCount < additionalCost.Amount)
        {
            var globalItemCount = globalInventory.GetItemCount(additionalCost.Item);
            var remainingNeeded = additionalCost.Amount - additionalResourceCount;
            var amountToContribute = Mathf.Min(globalItemCount, remainingNeeded);

            if (amountToContribute > 0)
            {
                for (int i = 0; i < amountToContribute; i++)
                {
                    globalInventory.RemoveItem(additionalCost.Item);
                }
                additionalResourceCount += amountToContribute;
                totalContributed += amountToContribute;
                Debug.Log($"Contributed {amountToContribute}x {additionalCost.Item.DisplayName} from global inventory. Progress: {additionalResourceCount}/{additionalCost.Amount}");
            }
        }

        return totalContributed;
    }

    /// <summary>
    /// Process automatic contributions from units during update
    /// </summary>
    public bool ProcessAutomaticContributions(NetworkRun networkRun, List<UnitInstance> units)
    {
        // Skip automatic contributions for free items (RequiredAmount == 0)
        if (RequiredAmount == 0)
        {
            return false;
        }

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

                // Prioritize primary resource if unit has it and it's not complete
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

                // Contribute additional resource if primary is done or unit doesn't have primary resource
                if (!contributed)
                {
                    var additionalCost = AdditionalResourceCost;
                    if (additionalCost != null && additionalCost.Item != null && additionalResourceCount < additionalCost.Amount)
                    {
                        var unitItemCount = unit.Inventory.GetItemCount(additionalCost.Item);
                        var remainingNeeded = additionalCost.Amount - additionalResourceCount;
                        var amountToContribute = Mathf.Min(itemsPerUpdate, unitItemCount, remainingNeeded);

                        if (amountToContribute > 0)
                        {
                            for (int i = 0; i < amountToContribute; i++)
                            {
                                unit.Inventory.RemoveItem(additionalCost.Item);
                            }
                            additionalResourceCount += amountToContribute;
                            contributed = true;
                            Debug.Log($"{unit.DisplayName} contributed {amountToContribute}x {additionalCost.Item.DisplayName}. Progress: {additionalResourceCount}/{additionalCost.Amount}");
                        }
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

    /// <summary>
    /// Get the contribution status for a unit
    /// </summary>
    public ContributionStatus GetUnitStatus(UnitInstance unit, bool isComplete, bool useUnitInventory)
    {
        // If already complete, show complete status
        if (isComplete)
        {
            return ContributionStatus.Complete;
        }

        // If requirements are met, show waiting status
        if (RequirementsMet())
        {
            return ContributionStatus.WaitingForReveal;
        }

        // Only check unit inventory in unit inventory mode
        if (useUnitInventory)
        {
            // Check if unit has either primary or additional resource
            var hasPrimary = requiredItem != null && currentResourceCount < RequiredAmount && unit?.Inventory != null && unit.Inventory.GetItemCount(requiredItem) > 0;
            var additionalCost = AdditionalResourceCost;
            var hasAdditional = additionalCost != null && additionalCost.Item != null &&
                               additionalResourceCount < additionalCost.Amount &&
                               unit?.Inventory != null && unit.Inventory.GetItemCount(additionalCost.Item) > 0;
            return (hasPrimary || hasAdditional) ? ContributionStatus.Contributing : ContributionStatus.NeedsResource;
        }

        // In global inventory mode, no per-unit status
        return ContributionStatus.NeedsResource;
    }

    /// <summary>
    /// Check if a unit has the required resource
    /// </summary>
    public bool UnitHasResource(UnitInstance unit)
    {
        if (unit?.Inventory == null) return false;

        // Check if unit has primary resource (and it's still needed)
        var hasPrimary = requiredItem != null && currentResourceCount < RequiredAmount && unit.Inventory.GetItemCount(requiredItem) > 0;
        if (hasPrimary) return true;

        // Check if unit has additional resource (and it's still needed)
        var additionalCost = AdditionalResourceCost;
        var hasAdditional = additionalCost != null && additionalCost.Item != null &&
                           additionalResourceCount < additionalCost.Amount &&
                           unit.Inventory.GetItemCount(additionalCost.Item) > 0;
        return hasAdditional;
    }

    /// <summary>
    /// Check if a contribute button should be shown for a unit
    /// </summary>
    public bool ShouldShowContributeButton(UnitInstance unit, bool isComplete, bool useUnitInventory)
    {
        // Don't show contribute button for free items
        if (RequiredAmount == 0)
        {
            return false;
        }
        return !isComplete && useUnitInventory && UnitHasResource(unit) && !RequirementsMet();
    }

    /// <summary>
    /// Check if a progress bar should be shown for a unit
    /// </summary>
    public bool ShouldShowProgress(UnitInstance unit, bool isComplete, bool useUnitInventory)
    {
        // Don't show progress for free items
        if (RequiredAmount == 0)
        {
            return false;
        }
        return UnitHasResource(unit) && !isComplete && useUnitInventory && !RequirementsMet();
    }

    /// <summary>
    /// Get normalized progress value (0-1) for progress bar
    /// </summary>
    public float GetNormalizedProgress(UnitInstance unit)
    {
        return GetUnitProgress(unit) / updateInterval;
    }

    /// <summary>
    /// Get display text for status labels
    /// </summary>
    public string GetStatusText(ContributionStatus status)
    {
        switch (status)
        {
            case ContributionStatus.Complete:
                return "✓ Task Complete";
            case ContributionStatus.Contributing:
                return "Contributing...";
            case ContributionStatus.WaitingForReveal:
                return "✓ Ready to reveal";
            case ContributionStatus.NeedsResource:
                // Show which resource is needed based on current progress
                if (currentResourceCount < RequiredAmount)
                {
                    var resourceName = requiredItem?.DisplayName ?? "resource";
                    return $"Needs {resourceName}";
                }
                else
                {
                    var additionalCost = AdditionalResourceCost;
                    if (additionalCost != null && additionalCost.Item != null)
                    {
                        return $"Needs {additionalCost.Item.DisplayName}";
                    }
                }
                return "Needs resource";
            default:
                return "";
        }
    }

    /// <summary>
    /// Get color for status text
    /// </summary>
    public Color GetStatusColor(ContributionStatus status)
    {
        switch (status)
        {
            case ContributionStatus.Complete:
                return new Color(1f, 0.85f, 0.3f); // Gold
            case ContributionStatus.Contributing:
                return new Color(0.7f, 1f, 0.7f); // Light green
            case ContributionStatus.WaitingForReveal:
                return new Color(0.5f, 1f, 0.5f); // Bright green
            case ContributionStatus.NeedsResource:
                return new Color(1f, 0.5f, 0.5f); // Light red
            default:
                return Color.white;
        }
    }
}
