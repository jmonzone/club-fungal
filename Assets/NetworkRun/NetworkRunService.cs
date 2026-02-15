using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Party
{
    [SerializeField] private List<UnitInstance> units = new List<UnitInstance>();
    public List<UnitInstance> Unit => units;

    public Party(List<UnitInstance> units)
    {
        this.units = units;
    }
}

[CreateAssetMenu(fileName = "NetworkRunService", menuName = "Club Fungal/Network Run/Network Run Service")]
public class NetworkRunService : GURUService
{
    [Header("References")]
    [SerializeField] private NetworkRunSettings settings;
    [SerializeField] private Inventory inventory;
    [SerializeField] private Party party;
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private UnitControllerService unitControllerService;

    [Header("Party Generation")]
    [SerializeField] private bool generateNewUnits = false;
    [SerializeField] private List<UnitSpecies> blacklistedSpecies;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnSpacing = 2f;

    private Transform spawnParent;
    private Vector3 partyCenterGround;
    private float maxTetherDistance;

    public event Action OnUnitReenteredTether;
    public NetworkRunSettings Settings => settings;
    public Inventory Inventory => inventory;
    public Party Party => party;
    public Vector3 PartyCenterGround => partyCenterGround;
    public float MaxTetherDistance => maxTetherDistance;

    public void SetSpawnParent(Transform parent)
    {
        spawnParent = parent;
    }

    public void SetPartyTetherData(Vector3 centerGround, float maxDistance)
    {
        partyCenterGround = centerGround;
        maxTetherDistance = maxDistance;
    }

    public void NotifyUnitReenteredTether()
    {
        OnUnitReenteredTether?.Invoke();
    }

    protected override void OnInitialize()
    {
        Debug.Log("Initializing NetworkRunService");
        inventory = new Inventory(9999999);
        party = new Party(GetParty());
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();
        SpawnParty();
    }

    private void SpawnParty()
    {
        if (party == null || party.Unit == null || party.Unit.Count == 0)
        {
            Debug.LogWarning("No party to spawn");
            return;
        }

        Vector3 basePosition = spawnParent != null ? spawnParent.position : Vector3.zero;

        for (int i = 0; i < party.Unit.Count; i++)
        {
            var unit = party.Unit[i];
            var offset = new Vector3(i * spawnSpacing, 0, 0);
            var position = basePosition + offset;
            unitControllerService.SpawnUnit(unit, position, spawnParent);
        }

        Debug.Log($"Spawned {party.Unit.Count} party members");
    }

    private List<UnitInstance> GetParty()
    {
        var partySize = settings.defaultPartySize;
        var party = new List<UnitInstance>();

        if (generateNewUnits)
        {
            // Generate brand new units (excluding blacklisted species)
            for (int i = 0; i < partySize; i++)
            {
                var newUnit = unitInstanceService.CreateUnit(species =>
                    blacklistedSpecies == null || !blacklistedSpecies.Contains(species));
                party.Add(newUnit);
            }
        }
        else
        {
            // Use existing units from the pool
            var allUnits = new List<UnitInstance>(unitInstanceService.Instances);
            for (int i = 0; i < Mathf.Min(partySize, allUnits.Count); i++)
            {
                var randomIndex = UnityEngine.Random.Range(0, allUnits.Count);
                var unitInstance = allUnits[randomIndex];
                allUnits.RemoveAt(randomIndex);
                party.Add(unitInstance);
            }
        }

        return party;
    }

    // public void Update()
    // {
    //     if (networkRun != null)
    //     {
    //         networkRun.UpdateSimulationTime();
    //     }
    // }
}
