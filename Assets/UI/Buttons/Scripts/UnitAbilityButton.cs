using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI button that activates the ability of the selected unit party leader.
/// Follows the project's service-oriented design pattern.
/// </summary>
public class UnitAbilityButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkRunPartyService partyService;
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
        if (partyService != null)
        {
            partyService.OnPartyLeaderChanged += OnPartyLeaderChanged;
        }
        UpdateButtonState();
    }

    private void OnDisable()
    {
        if (partyService != null)
        {
            partyService.OnPartyLeaderChanged -= OnPartyLeaderChanged;
        }
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void OnPartyLeaderChanged(UnitController newLeader)
    {
        UpdateButtonState();
    }

    private void OnButtonClicked()
    {
        if (partyService == null || partyService.PartyLeader == null) return;

        ActivateLeaderAbility(partyService.PartyLeader);
    }

    private void ActivateLeaderAbility(UnitController leader)
    {
        leader.Instance.Abilities[0].Activate();
    }

    private void UpdateButtonState()
    {
        if (partyService == null || partyService.PartyLeader == null)
        {
            button.interactable = false;
            if (buttonText != null)
            {
                buttonText.text = "No Leader";
            }
            HideCooldownUI();
            return;
        }

        var abilities = partyService.PartyLeader.Instance.Abilities;
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
