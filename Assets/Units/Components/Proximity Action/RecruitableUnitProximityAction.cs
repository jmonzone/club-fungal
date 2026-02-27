using UnityEngine;

/// <summary>
/// Proximity action for recruiting neutral units to the player's party.
/// Removes enemy status and optionally adds unit to party.
/// </summary>
[CreateAssetMenu(fileName = "RecruitableUnitAction", menuName = "Club Fungal/Proximity Actions/Recruitable Unit")]
public class RecruitableUnitProximityAction : ProximityAction
{
    [Header("Services")]
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private NetworkRunPartyService partyService;

    [Header("Settings")]
    [SerializeField] private bool addToParty = true;
    [SerializeField] private bool destroyAfterRecruit = false;

    public override void Execute(UnitController controller)
    {
        if (controller == null)
        {
            Debug.LogWarning("RecruitableUnitProximityAction: Controller is null");
            return;
        }

        Debug.Log($"Recruited {controller.name}!");

        // Register unit so it will be persisted
        if (controller.Instance != null && unitInstanceService != null)
        {
            if (!unitInstanceService.Instances.Contains(controller.Instance))
            {
                unitInstanceService.RegisterUnit(controller.Instance);
            }
        }

        // Add to player's party
        if (addToParty && partyService != null && controller.Instance != null)
        {
            if (!partyService.PartyControllers.Contains(controller))
            {
                partyService.AddUnitToParty(controller);
                Debug.Log($"Added {controller.Instance.DisplayName} to party");
            }
        }

        // Save to local data so recruited unit persists
        if (unitInstanceService != null)
        {
            unitInstanceService.SaveData();
            Debug.Log($"Saved recruited unit {controller.Instance?.DisplayName} to local data");
        }

        // Optionally destroy the unit after recruitment
        if (destroyAfterRecruit)
        {
            Object.Destroy(controller.gameObject);
        }

        // Optional: Play recruitment VFX/SFX here
        // Optional: Show confirmation UI here
    }
}
