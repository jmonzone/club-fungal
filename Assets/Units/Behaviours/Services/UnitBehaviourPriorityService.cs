using UnityEngine;

/// <summary>
/// Centralized service that manages all unit behavior priorities.
/// All priority rules are defined here in one place for easy debugging and maintenance.
/// </summary>
[CreateAssetMenu(fileName = "UnitBehaviourPriorityService", menuName = "Club Fungal/Units/Behaviour Priority Service")]
public class UnitBehaviourPriorityService : GURUService
{
    [Header("Services")]
    [SerializeField] private CameraService cameraService;

    [Header("Priority Values")]
    [SerializeField] private int combatPriority = 400;
    [SerializeField] private int returnToPartyPriority = 300;
    [SerializeField] private int foragePriority = 200;
    [SerializeField] private int wanderPriority = 100;

    protected override void OnInitialize()
    {
    }

    /// <summary>
    /// Master method to get priority for any behavior.
    /// Routes to appropriate priority method based on behavior type.
    /// </summary>
    public int GetBehaviourPriority(UnitBehaviour behaviour, UnitController controller)
    {
        // Route to appropriate priority service method based on behaviour type
        switch (behaviour)
        {
            case UnitCombat combat:
                return GetCombatPriority(combat);
            case UnitReturnToParty returnToParty:
                return GetReturnToPartyPriority(returnToParty, controller.transform.position, returnToParty.ReturnPosition);
            case UnitForage forage:
                return GetForagePriority(forage);
            case UnitWander wander:
                return GetWanderPriority();
            default:
                return 0; // Manually controlled behaviors
        }
    }

    /// <summary>
    /// Get priority for UnitCombat behavior.
    /// Rule: Active when target exists.
    /// </summary>
    public int GetCombatPriority(UnitCombat combat)
    {
        // Only active if we have a target
        return combat.CurrentTarget != null ? combatPriority : 0;
    }

    /// <summary>
    /// Get priority for UnitReturnToParty behavior.
    /// Rule: Active when camera is moving.
    /// </summary>
    public int GetReturnToPartyPriority(UnitReturnToParty returnBehaviour, Vector3 currentPosition, Vector3 returnPosition)
    {
        return cameraService.IsMoving ? returnToPartyPriority : 0;
    }

    /// <summary>
    /// Get priority for UnitForage behavior.
    /// Rule: Active when forage target exists.
    /// </summary>
    public int GetForagePriority(UnitForage forage)
    {
        // Active if we have a target
        return !cameraService.IsMoving && forage.CurrentTarget != null ? foragePriority : 0;
    }

    /// <summary>
    /// Get priority for UnitWander behavior.
    /// Rule: Default behavior (always active).
    /// </summary>
    public int GetWanderPriority()
    {
        // Default behavior - always active
        return !cameraService.IsMoving ? wanderPriority : 0;
    }
}
