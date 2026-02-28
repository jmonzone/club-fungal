using UnityEngine;

/// <summary>
/// Handles keyboard input for activating the selected unit's ability.
/// Now uses ControlModeService to determine which unit's ability to activate.
/// </summary>
public class NetworkRunAbilityInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ControlModeService controlModeService;

    [Header("Input Settings")]
    [SerializeField] private KeyCode abilityKey = KeyCode.Space;

    private void Update()
    {
        if (Input.GetKeyDown(abilityKey))
        {
            ActivateSelectedUnitAbility();
        }
    }

    private void ActivateSelectedUnitAbility()
    {
        if (controlModeService == null || controlModeService.SelectedUnit == null) return;

        var selectedUnit = controlModeService.SelectedUnit;
        if (selectedUnit.Abilities == null || selectedUnit.Abilities.Count == 0) return;

        var ability = selectedUnit.Abilities[0];
        if (ability != null && ability.CanActivate)
        {
            ability.Activate();
        }
    }
}
