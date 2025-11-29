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
    [SerializeField] private PlayerReference playerReference;

    [Header("Runtime")]
    [SerializeField] private List<UnitController> partyControllers = new List<UnitController>();

    public List<UnitController> PartyControllers => partyControllers;
    public event UnityAction<UnitController> OnUnitAddedToParty;
    public event UnityAction<UnitController> OnUnitRemovedFromParty;

    public void Initialize()
    {
        Debug.Log("Initializing PartyControllerService...");
        unitControllerService.Initialize();
        partyInstanceService.Initialize();
        partyControllers.Clear();
        if (unitControllerService != null && partyInstanceService != null)
        {
            Debug.Log("Populating party controllers from existing unit controllers and party instances.");
            foreach (var controller in unitControllerService.UnitControllers)
            {
                Debug.Log($"Checking unit controller {controller.Instance.DisplayName} for party membership.");
                if (controller != null && controller.Instance != null && partyInstanceService.PartyInstances.Any(p => p.Id == controller.Instance.Id))
                {
                    Debug.Log($"Adding existing party member {controller.Instance.DisplayName} to party controllers.");
                    AddToParty(controller);
                }
            }
        }
    }

    public void AddToParty(UnitController unit)
    {
        Debug.Log($"Adding unit {unit.Instance.DisplayName} to party controllers.");
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
            OnUnitAddedToParty?.Invoke(unit);
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
            OnUnitRemovedFromParty?.Invoke(unit);
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