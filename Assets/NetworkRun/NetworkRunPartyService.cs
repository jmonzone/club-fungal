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
    [SerializeField] private PlayerService playerService;
    [SerializeField] private LocalData localData;

    [Header("Party Generation")]
    [SerializeField] private bool useTemplates = false;
    [Tooltip("If useTemplates is true, these templates will be used to create the party")]
    [SerializeField] private List<UnitTemplate> partyTemplates = new List<UnitTemplate>();
    [SerializeField] private bool generateNewUnits = false;
    [SerializeField] private List<UnitSpecies> blacklistedSpecies;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnSpacing = 2f;

    [Header("Runtime")]
    [SerializeField] private NetworkRunParty party;
    private List<UnitController> partyControllers = new List<UnitController>();
    private UnitController partyLeader;
    private Transform spawnParent;

    private const string PARTY_KEY = "networkRunParty";

    public NetworkRunParty Party => party;
    public List<UnitController> PartyControllers => partyControllers;
    public UnitController PartyLeader => partyLeader;
    public Transform SpawnParent => spawnParent;

    public event UnityEngine.Events.UnityAction<UnitController> OnPartyLeaderChanged;

    public void SetSpawnParent(Transform parent)
    {
        spawnParent = parent;
    }

    protected override void OnInitialize()
    {
        Debug.Log("Initializing NetworkRunPartyService");

        // Try to load party from local data first
        var loadedParty = LoadPartyFromLocalData();

        if (loadedParty != null && loadedParty.Count > 0)
        {
            party = new NetworkRunParty(loadedParty);
            Debug.Log($"Loaded party from local data with {loadedParty.Count} members");
        }
        else
        {
            party = new NetworkRunParty(GenerateParty());
            SavePartyToLocalData();
        }
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
                SetPartyLeader(controller);
            }
        }

        Debug.Log($"Spawned {party.Units.Count} party members with {partyLeader?.Instance.DisplayName} as leader");
    }

    public void SetPartyLeader(UnitController newLeader)
    {
        if (newLeader == null) return;

        partyLeader = newLeader;

        // Update player service
        if (playerService != null)
        {
            playerService.SetPlayer(partyLeader);
        }

        OnPartyLeaderChanged?.Invoke(partyLeader);
    }

    public void CyclePartyLeader()
    {
        if (partyControllers == null || partyControllers.Count == 0)
        {
            Debug.LogWarning("No party controllers to cycle through");
            return;
        }

        // Find current leader index
        int currentIndex = partyControllers.IndexOf(partyLeader);

        // Cycle to next controller (wrap around to 0 if at end)
        int nextIndex = (currentIndex + 1) % partyControllers.Count;

        SetPartyLeader(partyControllers[nextIndex]);
        Debug.Log($"Party leader changed to: {partyLeader?.Instance.DisplayName}");
    }

    public void AddUnitToParty(UnitController controller)
    {
        if (controller == null || controller.Instance == null)
        {
            Debug.LogWarning("Cannot add null controller or controller without instance to party");
            return;
        }

        // Add to party instance if not already there
        if (!party.Units.Contains(controller.Instance))
        {
            party.Units.Add(controller.Instance);
        }

        // Add to party controllers if not already there
        if (!partyControllers.Contains(controller))
        {
            partyControllers.Add(controller);
        }

        Debug.Log($"Added {controller.Instance.DisplayName} to party. Party size: {partyControllers.Count}");

        // Save updated party to local data
        SavePartyToLocalData();
    }

    private List<UnitInstance> LoadPartyFromLocalData()
    {
        if (localData == null || localData.JsonFile == null)
        {
            return null;
        }

        if (!localData.JsonFile.ContainsKey(PARTY_KEY))
        {
            return null;
        }

        var partyIds = localData.JsonFile[PARTY_KEY] as Newtonsoft.Json.Linq.JArray;
        if (partyIds == null) return null;

        var partyUnits = new List<UnitInstance>();

        foreach (var idToken in partyIds)
        {
            var unitId = idToken.ToString();
            var unitInstance = unitInstanceService.Instances.Find(u => u.Data.id == unitId);

            if (unitInstance != null)
            {
                partyUnits.Add(unitInstance);
            }
            else
            {
                Debug.LogWarning($"Could not find unit with ID {unitId} in UnitInstanceService");
            }
        }

        return partyUnits;
    }

    private void SavePartyToLocalData()
    {
        if (localData == null || party == null)
        {
            return;
        }

        if (localData.JsonFile == null) localData.Initialize();

        // Save array of unit IDs
        var partyIds = new Newtonsoft.Json.Linq.JArray();
        foreach (var unit in party.Units)
        {
            if (unit != null && unit.Data != null)
            {
                partyIds.Add(unit.Data.id);
            }
        }

        localData.SaveData(PARTY_KEY, partyIds);
        Debug.Log($"Saved party to local data with {partyIds.Count} members");
    }

    private List<UnitInstance> GenerateParty()
    {
        var partySize = networkRunService.Settings.defaultPartySize;
        var newParty = new List<UnitInstance>();

        if (useTemplates && partyTemplates != null && partyTemplates.Count > 0)
        {
            // Create units from templates (or use existing instances if already created)
            foreach (var template in partyTemplates)
            {
                if (template != null)
                {
                    // Check if an instance from this template already exists
                    var existingUnit = unitInstanceService.Instances.Find(u => u.Template == template);
                    if (existingUnit != null)
                    {
                        newParty.Add(existingUnit);
                    }
                    else
                    {
                        var newUnit = unitInstanceService.CreateUnitFromTemplate(template);
                        newParty.Add(newUnit);
                    }
                }
            }
            Debug.Log($"Generated party from {partyTemplates.Count} templates");
        }
        else if (generateNewUnits)
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
