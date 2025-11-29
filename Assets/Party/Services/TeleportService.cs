using UnityEngine;

[CreateAssetMenu(fileName = "TeleportService", menuName = "Club Fungal/Teleport/Teleport Service")]
public class TeleportService : ScriptableObject
{
    [SerializeField] private PartyService partyService;

    public void TeleportParty(Vector3 position, Transform parent)
    {
        if (partyService == null) return;

        // Teleport all party members
        foreach (var member in partyService.PartyControllers)
        {
            member.Teleport(position, parent);
        }
    }
}