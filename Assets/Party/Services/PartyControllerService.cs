using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[CreateAssetMenu(fileName = "PartyControllerService", menuName = "Club Fungal/Party/Party Controller Service")]
public class PartyControllerService : GURUService
{
    [Header("References")]
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private PartyInstanceService partyInstanceService;
    [SerializeField] private PlayerService playerReference;

    [Header("Runtime")]
    [SerializeField] private List<UnitController> partyControllers = new List<UnitController>();

    public List<UnitController> PartyControllers => partyControllers;

    protected override void OnInitialize()
    {
        partyControllers.Clear();

        foreach (var controller in unitControllerService.Controllers)
        {
            if (partyInstanceService.PartyInstances.Any(p => p.Id == controller.Instance.Id))
            {
                AddToParty(controller);
            }
        }
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        foreach (var controller in unitControllerService.Controllers)
        {
            if (partyInstanceService.PartyInstances.Any(p => p.Id == controller.Instance.Id))
            {
                controller.OnNavMeshAgentReady += () =>
                {
                    AddToParty(controller);
                };
            }
        }
    }


    public void AddToParty(UnitController unit)
    {
        // Debug.Log($"Adding unit {unit.Instance.DisplayName} to party controllers.");
        if (!partyControllers.Contains(unit))
        {
            // Ensure the instance is in the party
            if (!partyInstanceService.PartyInstances.Contains(unit.Instance))
            {
                partyInstanceService.AddToParty(unit);
            }
            partyControllers.Add(unit);

            if (Application.isPlaying && unit.gameObject != playerReference.Player.gameObject)
            {
                var followBehaviour = unit.GetComponent<UnitFollow>();
                if (followBehaviour != null)
                {
                    followBehaviour.SetTarget(playerReference.Player.transform);
                    unit.SetBehaviour(followBehaviour);
                }
            }
        }
    }

    public void RemoveFromParty(UnitController unit)
    {
        if (partyControllers.Contains(unit))
        {
            partyControllers.Remove(unit);
            // Optionally stop follow behavior
            var followBehaviour = unit.GetComponent<UnitFollow>();
            if (followBehaviour != null)
            {
                followBehaviour.StopFollowing();
            }
        }
    }

    // Additional functionality for handling follow behaviors
    public void UpdateFollowTargets(Transform newTarget)
    {
        foreach (var controller in partyControllers)
        {
            var followBehaviour = controller.GetComponent<UnitFollow>();
            if (followBehaviour != null)
            {
                followBehaviour.SetTarget(newTarget);
            }
        }
    }
}