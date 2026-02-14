using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkRunService", menuName = "Club Fungal/Network Run/Network Run Service")]
public class NetworkRunService : GURUService
{
    [Header("References")]
    [SerializeField] private NetworkRunSettings settings;
    [SerializeField] private Inventory inventory;

    public Inventory Inventory => inventory;

    protected override void OnInitialize()
    {
        Debug.Log("Initializing NetworkRunService");
        inventory = new Inventory(9999999);
    }

    // private List<DoorCondition> LoadAllDoorConditions()
    // {
    //     var doorConditions = new List<DoorCondition>();
    //     var conditions = Resources.LoadAll<DoorCondition>("");
    //     doorConditions.AddRange(conditions);
    //     return doorConditions;
    // }

    // private List<UnitInstance> GetParty()
    // {
    //     var partySize = settings.defaultPartySize;
    //     var party = new List<UnitInstance>();

    //     var allUnits = new List<UnitInstance>(unitInstanceService.Units);
    //     for (int i = 0; i < Mathf.Min(partySize, allUnits.Count); i++)
    //     {
    //         var randomIndex = Random.Range(0, allUnits.Count);
    //         var unitTemplate = allUnits[randomIndex];
    //         allUnits.RemoveAt(randomIndex);

    //         var unitInstance = new UnitInstance(unitTemplate.Template);
    //         unitInstance.SetTemplate(unitTemplate.Template);
    //         party.Add(unitInstance);
    //     }

    //     return party;
    // }

    // public void Update()
    // {
    //     if (networkRun != null)
    //     {
    //         networkRun.UpdateSimulationTime();
    //     }
    // }
}
