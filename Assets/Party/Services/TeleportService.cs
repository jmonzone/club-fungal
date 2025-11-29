using UnityEngine;

[CreateAssetMenu(fileName = "TeleportService", menuName = "Club Fungal/Teleport/Teleport Service")]
public class TeleportService : ScriptableObject
{
    [SerializeField] private PartyControllerService partyControllerService;

    public void TeleportParty(Vector3 position, Transform parent)
    {
        if (partyControllerService == null) return;

        // Teleport all party members
        foreach (var member in partyControllerService.PartyControllers)
        {
            member.Teleport(position, parent);
        }
    }
}