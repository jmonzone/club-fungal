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
    public ResourceCost AdditionalResourceCost => additionalResourceCost;
    public float UpdateInterval => updateInterval;
    public int ItemsPerUpdate => itemsPerUpdate;

    public int RequiredAmount
    {
        get
        {
            // Use fixed amount if set
            if (fixedAmount >= 0)
            {
                return fixedAmount;
            }

            // Use scaling config if available
            if (scalingConfig != null)
            {
                return scalingConfig.GetCost(currentIndex);
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
        this.settings = settings;
        this.scalingConfig = scalingConfig;
        this.additionalResourceCost = null; // No longer using additional resource costs from settings

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
            var hasResource = requiredItem != null && unit?.Inventory != null && unit.Inventory.GetItemCount(requiredItem) > 0;
            return hasResource ? ContributionStatus.Contributing : ContributionStatus.NeedsResource;
        }

        // In global inventory mode, no per-unit status
        return ContributionStatus.NeedsResource;
    }

    /// <summary>
    /// Check if a unit has the required resource
    /// </summary>
    public bool UnitHasResource(UnitInstance unit)
    {
        return requiredItem != null && unit?.Inventory != null && unit.Inventory.GetItemCount(requiredItem) > 0;
    }

    /// <summary>
    /// Check if a contribute button should be shown for a unit
    /// </summary>
    public bool ShouldShowContributeButton(UnitInstance unit, bool isComplete, bool useUnitInventory)
    {
        return !isComplete && useUnitInventory && UnitHasResource(unit) && !RequirementsMet();
    }

    /// <summary>
    /// Check if a progress bar should be shown for a unit
    /// </summary>
    public bool ShouldShowProgress(UnitInstance unit, bool isComplete, bool useUnitInventory)
    {
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
                var resourceName = requiredItem?.DisplayName ?? "resource";
                return $"Needs {resourceName}";
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
