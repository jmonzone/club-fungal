using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SummonUnit Component", menuName = "Club Fungal/Activities/Components/SummonUnit")]
public class SummonUnitComponent : ActivityComponent
{
    [SerializeField] private ResourceContributionHandler contributionHandler = new ResourceContributionHandler();
    [SerializeField] private List<UnitInstance> summonedUnits = new List<UnitInstance>();

    [Header("Summon Settings")]
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private int maxSummons = -1; // -1 = unlimited

    // Expose contribution handler properties
    public int CurrentResourceCount => contributionHandler.CurrentResourceCount;
    public ResourceCost AdditionalResourceCost => contributionHandler.AdditionalResourceCost;
    public int AdditionalResourceCount => contributionHandler.AdditionalResourceCount;
    public int RequiredAmount => contributionHandler.RequiredAmount;
    public float UpdateInterval => contributionHandler.UpdateInterval;
    public int ItemsPerUpdate => contributionHandler.ItemsPerUpdate;
    public ItemTemplate RequiredItem => contributionHandler.RequiredItem;
    public List<UnitInstance> SummonedUnits => summonedUnits;
    public int SummonedCount => summonedUnits?.Count ?? 0;
    public int MaxSummons => maxSummons;

    public float GetUnitProgress(UnitInstance unit)
    {
        return contributionHandler.GetUnitProgress(unit);
    }

    public bool CanSummon()
    {
        if (maxSummons > 0 && summonedUnits.Count >= maxSummons)
        {
            return false;
        }
        return contributionHandler.RequirementsMet();
    }

    public void ContributeFromUnit(UnitInstance unit)
    {
        contributionHandler.ContributeFromUnit(unit);
    }

    public int ContributeFromGlobalInventory(Inventory globalInventory)
    {
        return contributionHandler.ContributeFromGlobalInventory(globalInventory);
    }

    public UnitInstance SummonUnit(NetworkRun networkRun)
    {
        if (!CanSummon())
        {
            Debug.LogWarning("[SummonUnit] Cannot summon: requirements not met or max summons reached");
            return null;
        }

        // Create random unit (query = null means any unit)
        var newUnit = unitInstanceService.CreateUnit(null);
        summonedUnits.Add(newUnit);

        // Add to party
        if (networkRun?.Party != null)
        {
            networkRun.Party.Add(newUnit);
        }

        // Reset contribution progress for next summon
        contributionHandler.Reset();

        Debug.Log($"[SummonUnit] Summoned {newUnit.DisplayName}! Total summoned: {summonedUnits.Count}");
        return newUnit;
    }

    protected override void OnInitialize()
    {
        // Initialize contribution handler with settings from networkRun
        if (networkRun?.Settings != null)
        {
            contributionHandler.Initialize(0, networkRun.Settings); // Use 0 as context index
        }
        summonedUnits.Clear();
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Check contribution mode from settings
        var useGlobalInventory = networkRun?.Settings?.zoneContributionMode == ResourceCollectionMode.GlobalInventory;

        // Global inventory mode uses manual contributions only (via button clicks)
        if (useGlobalInventory)
        {
            return;
        }

        // Automatic contributions from unit inventories
        contributionHandler.ProcessAutomaticContributions(networkRun, activityInstance?.Units);

        // Note: Auto-summoning is not triggered here - requires manual button click
    }
}
