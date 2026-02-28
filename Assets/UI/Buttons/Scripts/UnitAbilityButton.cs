using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI button that activates the ability of the selected unit.
/// Listens to ControlModeService to track which unit is selected.
/// </summary>
public class UnitAbilityButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ControlModeService controlModeService;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private List<Image> chargeIndicators;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (buttonText == null) buttonText = GetComponentInChildren<TextMeshProUGUI>();

        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnEnable()
    {
        if (controlModeService != null)
        {
            controlModeService.OnModeChanged += OnModeChanged;
            controlModeService.OnUnitSelected += OnUnitSelected;
            controlModeService.OnUnitDeselected += OnUnitDeselected;
        }
        UpdateButtonState();
    }

    private void OnDisable()
    {
        if (controlModeService != null)
        {
            controlModeService.OnModeChanged -= OnModeChanged;
            controlModeService.OnUnitSelected -= OnUnitSelected;
            controlModeService.OnUnitDeselected -= OnUnitDeselected;
        }
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void OnModeChanged(ControlModeService.ControlMode mode)
    {
        UpdateButtonState();
    }

    private void OnUnitSelected(UnitInstance unit)
    {
        UpdateButtonState();
    }

    private void OnUnitDeselected()
    {
        UpdateButtonState();
    }

    private void OnButtonClicked()
    {
        if (controlModeService == null || controlModeService.SelectedUnit == null) return;

        ActivateSelectedUnitAbility();
    }

    private void ActivateSelectedUnitAbility()
    {
        var abilities = controlModeService.SelectedUnit.Abilities;
        if (abilities != null && abilities.Count > 0)
        {
            abilities[0].Activate();
        }
    }

    private void UpdateButtonState()
    {
        if (controlModeService == null || controlModeService.SelectedUnit == null)
        {
            button.interactable = false;
            if (buttonText != null)
            {
                buttonText.text = "No Unit";
            }
            HideCooldownUI();
            return;
        }

        var abilities = controlModeService.SelectedUnit.Abilities;
        if (abilities == null || abilities.Count == 0)
        {
            button.interactable = false;
            if (buttonText != null)
            {
                buttonText.text = "No Ability";
            }
            HideCooldownUI();
            return;
        }

        var ability = abilities[0];
        button.interactable = ability.CanActivate;

        if (buttonText != null)
        {
            buttonText.text = ability.Definition.DisplayName;
        }

        UpdateCooldownUI(ability);
        UpdateChargeUI(ability);
    }

    private void UpdateCooldownUI(AbilityInstance ability)
    {
        // For charge-based abilities, show charge regen progress instead
        if (ability is DashAbilityInstance dashAbility)
        {
            var isRegeneratingCharge = dashAbility.CurrentCharges < dashAbility.DashDefinition.MaxCharges;

            if (cooldownFill != null)
            {
                cooldownFill.gameObject.SetActive(isRegeneratingCharge);
                if (isRegeneratingCharge)
                {
                    cooldownFill.fillAmount = 1f - (dashAbility.ChargeRegenTimer / dashAbility.DashDefinition.ChargeRegenTime);
                }
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(isRegeneratingCharge);
                if (isRegeneratingCharge)
                {
                    var timeRemaining = dashAbility.DashDefinition.ChargeRegenTime - dashAbility.ChargeRegenTimer;
                    cooldownText.text = Mathf.CeilToInt(timeRemaining).ToString();
                }
            }
            return;
        }

        // Standard cooldown for non-charge abilities
        var hasCooldown = ability.CooldownRemaining > 0f;

        if (cooldownFill != null)
        {
            cooldownFill.gameObject.SetActive(hasCooldown);
            if (hasCooldown)
            {
                var maxCooldown = GetMaxCooldown(ability);
                cooldownFill.fillAmount = ability.CooldownRemaining / maxCooldown;
            }
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(hasCooldown);
            if (hasCooldown)
            {
                cooldownText.text = Mathf.CeilToInt(ability.CooldownRemaining).ToString();
            }
        }
    }

    private float GetMaxCooldown(AbilityInstance ability)
    {
        if (ability is DashAbilityInstance dashAbility)
        {
            return dashAbility.DashDefinition.ChargeRegenTime;
        }

        // Default fallback - adjust based on your ability types
        return 5f;
    }

    private void UpdateChargeUI(AbilityInstance ability)
    {
        if (chargeIndicators == null || chargeIndicators.Count == 0) return;

        // Check if it's a dash ability (or any charge-based ability)
        if (ability is DashAbilityInstance dashAbility)
        {
            var maxCharges = dashAbility.DashDefinition.MaxCharges;
            var currentCharges = dashAbility.CurrentCharges;

            for (int i = 0; i < chargeIndicators.Count; i++)
            {
                if (i < maxCharges)
                {
                    chargeIndicators[i].gameObject.SetActive(true);
                    chargeIndicators[i].enabled = i < currentCharges;
                }
                else
                {
                    chargeIndicators[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            foreach (var indicator in chargeIndicators)
            {
                indicator.gameObject.SetActive(false);
            }
        }
    }

    private void HideCooldownUI()
    {
        if (cooldownFill != null) cooldownFill.gameObject.SetActive(false);
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
        if (chargeIndicators != null)
        {
            foreach (var indicator in chargeIndicators)
            {
                indicator.gameObject.SetActive(false);
            }
        }
    }
}
