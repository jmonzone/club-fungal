using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NetworkRunParty
{
    [SerializeField] private List<UnitInstance> units = new List<UnitInstance>();
    public List<UnitInstance> Units => units;

    public NetworkRunParty(List<UnitInstance> units)
    {
        this.units = units;
    }
}

[CreateAssetMenu(fileName = "NetworkRunPartyService", menuName = "Club Fungal/Network Run/Network Run Party Service")]
public class NetworkRunPartyService : GURUService
{
    [Header("References")]
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private UnitControllerService unitControllerService;

    [Header("Party Generation")]
    [SerializeField] private bool generateNewUnits = false;
    [SerializeField] private List<UnitSpecies> blacklistedSpecies;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnSpacing = 2f;

    [Header("Runtime")]
    [SerializeField] private NetworkRunParty party;
    private List<UnitController> partyControllers = new List<UnitController>();
    private UnitController partyLeader;
    private Transform spawnParent;

    public NetworkRunParty Party => party;
    public List<UnitController> PartyControllers => partyControllers;
    public UnitController PartyLeader => partyLeader;
    public Transform SpawnParent => spawnParent;

    public void SetSpawnParent(Transform parent)
    {
        spawnParent = parent;
    }

    protected override void OnInitialize()
    {
        Debug.Log("Initializing NetworkRunPartyService");
        party = new NetworkRunParty(GenerateParty());
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();
        SpawnParty();
    }

    public void SpawnParty()
    {
        if (party == null || party.Units == null || party.Units.Count == 0)
        {
            Debug.LogWarning("No party to spawn");
            return;
        }

        partyControllers.Clear();
        Vector3 basePosition = spawnParent != null ? spawnParent.position : Vector3.zero;

        for (int i = 0; i < party.Units.Count; i++)
        {
            var unit = party.Units[i];
            var offset = new Vector3(i * spawnSpacing, 0, 0);
            var position = basePosition + offset;
            var controller = unitControllerService.SpawnUnit(unit, position);
            partyControllers.Add(controller);

            if (i == 0)
            {
                partyLeader = controller;
            }
        }

        Debug.Log($"Spawned {party.Units.Count} party members with {partyLeader?.Instance.DisplayName} as leader");
    }

    private List<UnitInstance> GenerateParty()
    {
        var partySize = networkRunService.Settings.defaultPartySize;
        var newParty = new List<UnitInstance>();

        if (generateNewUnits)
        {
            // Generate brand new units (excluding blacklisted species)
            for (int i = 0; i < partySize; i++)
            {
                var newUnit = unitInstanceService.CreateUnit(species =>
                    blacklistedSpecies == null || !blacklistedSpecies.Contains(species));
                newParty.Add(newUnit);
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
                newParty.Add(unitInstance);
            }
        }

        return newParty;
    }
}
