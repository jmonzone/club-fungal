using UnityEngine;

[CreateAssetMenu(fileName = "TeleportService", menuName = "Club Fungal/Teleport/Teleport Service")]
public class TeleportService : ScriptableObject
{
    [SerializeField] private PartyControllerService partyControllerService;
    [SerializeField] private UnitBehaviourPriorityService behaviourPriorityService;

    public void TeleportParty(Vector3 position, Transform parent)
    {
        // Debug.Log($"Teleporting party to position {position} with parent {parent}.");
        // Teleport all party members
        foreach (var member in partyControllerService.PartyControllers)
        {
            // Skip units that are assigned to work (e.g., cooking)
            if (behaviourPriorityService != null && behaviourPriorityService.IsAssignedToWork(member))
            {
                continue;
            }

            // Debug.Log($"Teleporting member {member.Instance}.");
            member.Teleport(position, parent);
        }
    }
}