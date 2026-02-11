using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ZoneOcclusion", menuName = "Club Fungal/Activities/Components/ZoneOcclusion")]
public class ZoneOcclusion : ActivityComponent
{
    [SerializeField] private ActivityComponent hiddenZoneComponent;
    [SerializeField] private bool isRevealed = false;
    [SerializeField] private ResourceContributionHandler contributionHandler = new ResourceContributionHandler();

    [Header("Occlusion Settings")]
    [SerializeField] private Sprite occludedSprite; // Icon to show when zone is hidden
    [SerializeField] private string occludedDisplayName = "Hidden Zone"; // Name to show when zone is hidden

    // Expose contribution handler properties
    public int CurrentResourceCount => contributionHandler.CurrentResourceCount;
    public ResourceCost AdditionalResourceCost => contributionHandler.AdditionalResourceCost;
    public int AdditionalResourceCount => contributionHandler.AdditionalResourceCount;
    public int RequiredAmount => contributionHandler.RequiredAmount;
    public bool IsRevealed => isRevealed;
    public float UpdateInterval => contributionHandler.UpdateInterval;
    public int ItemsPerUpdate => contributionHandler.ItemsPerUpdate;
    public ItemTemplate RequiredItem => contributionHandler.RequiredItem;
    public Sprite OccludedSprite => occludedSprite;
    public string OccludedDisplayName => occludedDisplayName;
    public ActivityComponent HiddenComponent => hiddenZoneComponent;

    public override void RemapReferences(Dictionary<ActivityComponent, ActivityComponent> originalToCopy)
    {
        if (hiddenZoneComponent != null && originalToCopy.TryGetValue(hiddenZoneComponent, out var remapped))
        {
            hiddenZoneComponent = remapped;
        }
    }

    public override bool ShouldUpdate()
    {
        // ZoneOcclusion updates when hidden, skips when revealed
        return !isRevealed;
    }

    public override ActivityComponent[] GetControlledComponents()
    {
        // When zone is hidden, we control the hidden component (it shouldn't update)
        if (!isRevealed && hiddenZoneComponent != null)
        {
            return new ActivityComponent[] { hiddenZoneComponent };
        }
        return null;
    }

    public float GetUnitProgress(UnitInstance unit)
    {
        return contributionHandler.GetUnitProgress(unit);
    }

    public ResourceContributionHandler GetContributionHandler()
    {
        return contributionHandler;
    }

    /// <summary>
    /// Sets the zone occlusion configuration with settings reference for dynamic cost updates
    /// </summary>
    public void SetZoneOcclusion(ActivityComponent nextComponent, int zoneIndex, NetworkRunSettings settings)
    {
        hiddenZoneComponent = nextComponent;
        isRevealed = false;
        contributionHandler.Initialize(zoneIndex, settings);

        var costText = $"{RequiredAmount}x {RequiredItem?.DisplayName}";
        if (AdditionalResourceCost != null && AdditionalResourceCost.Item != null)
        {
            costText += $" + {AdditionalResourceCost.Amount}x {AdditionalResourceCost.Item.DisplayName}";
        }
        Debug.Log($"[ZoneOcclusion] Zone {zoneIndex} hidden. Requires {costText} to reveal.");
    }

    public void ContributeFromUnit(UnitInstance unit)
    {
        if (isRevealed) return;
        contributionHandler.ContributeFromUnit(unit);
    }

    public int ContributeFromGlobalInventory(Inventory globalInventory)
    {
        if (isRevealed) return 0;
        return contributionHandler.ContributeFromGlobalInventory(globalInventory);
    }

    protected override void OnInitialize()
    {
        contributionHandler.Reset();
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // If zone is revealed, the hidden component will update itself (no proxy needed)
        if (isRevealed)
        {
            return;
        }

        // Check contribution mode from settings
        var useGlobalInventory = networkRun?.Settings?.zoneContributionMode == ResourceCollectionMode.GlobalInventory;

        // Global inventory mode uses manual contributions only (via button clicks)
        if (useGlobalInventory)
        {
            return;
        }

        // Automatic contributions from unit inventories
        contributionHandler.ProcessAutomaticContributions(networkRun, activityInstance?.Units);
    }

    public void RevealZone()
    {
        if (!isRevealed && contributionHandler.RequirementsMet())
        {
            isRevealed = true;
            var costText = $"{CurrentResourceCount}x {RequiredItem?.DisplayName}";
            if (AdditionalResourceCost != null && AdditionalResourceCost.Item != null)
            {
                costText += $" + {AdditionalResourceCount}x {AdditionalResourceCost.Item.DisplayName}";
            }
            Debug.Log($"[ZoneOcclusion] Zone revealed! Used {costText}");
        }
    }
}
