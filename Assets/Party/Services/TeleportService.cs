using UnityEngine;

[CreateAssetMenu(fileName = "TeleportService", menuName = "Club Fungal/Teleport/Teleport Service")]
public class TeleportService : ScriptableObject
{
    [SerializeField] private PartyControllerService partyControllerService;

    public void TeleportParty(Vector3 position, Transform parent)
    {
        // Debug.Log($"Teleporting party to position {position} with parent {parent}.");
        // Teleport all party members
        foreach (var member in partyControllerService.PartyControllers)
        {
            // Debug.Log($"Teleporting member {member.Instance}.");
            member.Teleport(position, parent);
        }
    }
}