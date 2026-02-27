using UnityEngine;

/// <summary>
/// Handles keyboard input for activating the party leader's ability during network runs.
/// </summary>
public class NetworkRunAbilityInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkRunPartyService partyService;

    [Header("Input Settings")]
    [SerializeField] private KeyCode abilityKey = KeyCode.Space;

    private void Update()
    {
        if (Input.GetKeyDown(abilityKey))
        {
            ActivatePartyLeaderAbility();
        }
    }

    private void ActivatePartyLeaderAbility()
    {
        if (partyService == null || partyService.PartyLeader == null) return;

        var leader = partyService.PartyLeader;
        if (leader.Instance == null) return;
        if (leader.Instance.Abilities == null || leader.Instance.Abilities.Count == 0) return;

        var ability = leader.Instance.Abilities[0];
        if (ability != null && ability.CanActivate)
        {
            ability.Activate();
        }
    }
}
