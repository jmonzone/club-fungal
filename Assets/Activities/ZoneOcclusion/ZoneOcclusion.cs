using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ZoneOcclusion", menuName = "Club Fungal/Activities/Components/ZoneOcclusion")]
public class ZoneOcclusion : ActivityComponent
{
    private ActivityComponent hiddenZoneComponent;
    [SerializeField] private int currentResourceCount;
    [SerializeField] private bool isRevealed = false;
    private Dictionary<UnitInstance, float> unitProgress = new Dictionary<UnitInstance, float>();
    
    // Dynamic cost calculation
    private NetworkRunSettings settings;
    private int zoneIndex = -1;
    [SerializeField] private int cachedRequiredAmount;

    [Header("Collection Settings")]
    [SerializeField] private float updateInterval = 5f;
    [SerializeField] private int itemsPerUpdate = 1;

    [Header("Occlusion Settings")]
    [SerializeField] private ItemTemplate requiredItem; // Spores by default
    [SerializeField] private ResourceCost additionalResourceCost; // Optional additional resource requirement
    [SerializeField] private int additionalResourceCount; // Progress toward additional resource requirement
    [SerializeField] private Sprite occludedSprite; // Icon to show when zone is hidden
    [SerializeField] private string occludedDisplayName = "Hidden Zone"; // Name to show when zone is hidden

    public int CurrentResourceCount => currentResourceCount;
    public ResourceCost AdditionalResourceCost => additionalResourceCost;
    public int AdditionalResourceCount => additionalResourceCount;

    public int RequiredAmount
    {
        get
        {
            // Dynamically calculate from settings if available
            if (settings != null && zoneIndex >= 0)
            {
                return settings.GetZoneCost(zoneIndex);
            }
            // Fall back to cached value
            return cachedRequiredAmount;
        }
    }
    
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

    /// <summary>
    /// Sets the zone occlusion configuration with settings reference for dynamic cost updates
    /// </summary>
    public void SetZoneOcclusion(ActivityComponent nextComponent, int zoneIndex, NetworkRunSettings settings)
    {
        hiddenZoneComponent = nextComponent;
        currentResourceCount = 0;
        additionalResourceCount = 0;
        isRevealed = false;
        this.zoneIndex = zoneIndex;
        this.settings = settings;
        this.cachedRequiredAmount = settings?.GetZoneCost(zoneIndex) ?? 0;
        this.additionalResourceCost = settings?.GetAdditionalResourceCost(zoneIndex);

        var costText = $"{RequiredAmount}x {requiredItem?.DisplayName}";
        if (additionalResourceCost != null && additionalResourceCost.Item != null)
        {
            costText += $" + {additionalResourceCost.Amount}x {additionalResourceCost.Item.DisplayName}";
        }
        Debug.Log($"[ZoneOcclusion] Zone {zoneIndex} hidden. Requires {costText} to reveal.");
    }

    public void ContributeFromUnit(UnitInstance unit)
    {
        if (unit?.Inventory == null || isRevealed) return;

        // Contribute primary resource (spores)
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

    public int ContributeFromGlobalInventory(Inventory globalInventory)
    {
        if (globalInventory == null || isRevealed) return 0;

        int totalContributed = 0;

        // Contribute primary resource (spores)
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

        // Don't automatically reveal - require manual button click
        return totalContributed;
    }

    protected override void OnInitialize()
    {
        unitProgress.Clear();
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Don't process if already revealed
        if (isRevealed) return;

        // Check contribution mode from settings
        var useGlobalInventory = networkRun?.Settings?.zoneContributionMode == ResourceCollectionMode.GlobalInventory;

        // Global inventory mode uses manual contributions only (via button clicks)
        if (useGlobalInventory)
        {
            return;
        }

        // Automatic contributions from unit inventories
        if (activityInstance?.Units == null) return;

        foreach (var unit in activityInstance.Units)
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

                // Contribute primary resource (spores)
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

                    // Don't automatically reveal - require manual button click
                    // Just reset progress after contributing
                }
            }
        }
    }

    public void RevealZone()
    {
        bool primaryComplete = currentResourceCount >= RequiredAmount;
        bool additionalComplete = additionalResourceCost == null || additionalResourceCount >= additionalResourceCost.Amount;

        if (!isRevealed && primaryComplete && additionalComplete)
        {
            isRevealed = true;
            var costText = $"{currentResourceCount}x {requiredItem?.DisplayName}";
            if (additionalResourceCost != null && additionalResourceCost.Item != null)
            {
                costText += $" + {additionalResourceCount}x {additionalResourceCost.Item.DisplayName}";
            }
            Debug.Log($"[ZoneOcclusion] Zone revealed! Used {costText}");
        }
    }
}
