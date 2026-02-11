using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SummonUnit Component", menuName = "Club Fungal/Activities/Components/SummonUnit")]
public class SummonUnitComponent : ActivityComponent
{
    [SerializeField] private ResourceContributionHandler contributionHandler = new ResourceContributionHandler();
    [SerializeField] private List<UnitInstance> summonedUnits = new List<UnitInstance>();

    [Header("Summon Settings")]
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private List<UnitSpecies> speciesToSummon = new List<UnitSpecies>(); // Species to create, empty = random
    [SerializeField] private ResourceScalingConfig scalingConfig; // Cost scaling configuration
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
    public List<UnitSpecies> SpeciesToSummon => speciesToSummon;

    public float GetUnitProgress(UnitInstance unit)
    {
        return contributionHandler.GetUnitProgress(unit);
    }

    public ResourceContributionHandler GetContributionHandler()
    {
        return contributionHandler;
    }

    public bool CanSummon()
    {
        if (maxSummons > 0 && summonedUnits.Count >= maxSummons)
        {
            return false;
        }

        // Free items (RequiredAmount == 0) are always claimable
        if (contributionHandler.RequiredAmount == 0)
        {
            return true;
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

    public UnitInstance SummonUnit(NetworkRun networkRun, ActivityInstance activityInstance = null)
    {
        if (!CanSummon())
        {
            Debug.LogWarning("[SummonUnit] Cannot summon: requirements not met or max summons reached");
            return null;
        }

        // Create unit based on specified species list (empty = random)
        UnitInstanceService.UnitQuery query = null;
        UnitSpecies selectedSpecies = null;

        if (speciesToSummon != null && speciesToSummon.Count > 0)
        {
            // Randomly select from the list
            selectedSpecies = speciesToSummon[Random.Range(0, speciesToSummon.Count)];
            query = (species => species == selectedSpecies);
        }

        var newUnit = unitInstanceService.CreateUnit(query);
        summonedUnits.Add(newUnit);

        // Add to party
        if (networkRun?.Party != null)
        {
            networkRun.Party.Add(newUnit);
        }

        // Add to this summon activity
        if (activityInstance != null)
        {
            activityInstance.AddUnit(newUnit);
        }

        // Reset contribution progress and scale for next summon
        contributionHandler.Reset();

        // Update index for next summon
        // If firstItemFree: 1st=free, 2nd=lvl2(8), 3rd=lvl3(17), formula: count+1
        // Without: 1st=lvl2(8), 2nd=lvl3(17), 3rd=lvl4(25), formula: count+2
        if (scalingConfig != null && scalingConfig.firstItemFree)
        {
            contributionHandler.UpdateIndex(summonedUnits.Count + 1);
        }
        else
        {
            contributionHandler.UpdateIndex(summonedUnits.Count + 2);
        }

        var speciesName = selectedSpecies != null ? selectedSpecies.name : "random species";
        Debug.Log($"[SummonUnit] Summoned {newUnit.DisplayName} ({speciesName})! Total summoned: {summonedUnits.Count}. Next summon requires {RequiredAmount} resources.");
        return newUnit;
    }

    protected override void OnInitialize()
    {
        summonedUnits.Clear();

        // Initialize with scaling config (firstItemFree in config determines if first summon is free)
        if (networkRun?.Settings != null)
        {
            contributionHandler.Initialize(0, networkRun.Settings, scalingConfig);
        }
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
