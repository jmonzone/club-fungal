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

    [Header("Settings")]
    [SerializeField] private float cooldownDuration = 5f;

    [Header("Runtime")]
    [SerializeField] private float cooldownTimer = 0f;
    [SerializeField] private bool isOnCooldown = false;

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
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                UpdateButtonState();
            }
            else
            {
                UpdateCooldownText();
            }
        }
    }

    private void OnPartyLeaderChanged(UnitController newLeader)
    {
        UpdateButtonState();
    }

    private void OnButtonClicked()
    {
        if (partyService == null || partyService.PartyLeader == null)
        {
            Debug.LogWarning("No party leader available to activate ability");
            return;
        }

        if (isOnCooldown)
        {
            Debug.Log("Ability is on cooldown");
            return;
        }

        ActivateLeaderAbility(partyService.PartyLeader);
        StartCooldown();
    }

    private void ActivateLeaderAbility(UnitController leader)
    {
        leader.Instance.Abilities[0].Activate(leader);
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownTimer = cooldownDuration;
        UpdateButtonState();
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
            return;
        }

        button.interactable = !isOnCooldown;

        if (buttonText != null && !isOnCooldown)
        {
            buttonText.text = partyService.PartyLeader.Instance.Abilities[0].Definition.DisplayName;
        }
    }

    private void UpdateCooldownText()
    {
        if (buttonText != null)
        {
            buttonText.text = $"Cooldown: {Mathf.CeilToInt(cooldownTimer)}s";
        }
    }
}
