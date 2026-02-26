using UnityEngine;

/// <summary>
/// Ability that allows units to dash forward with charge system.
/// </summary>
[CreateAssetMenu(fileName = "DashAbility", menuName = "Club Fungal/Abilities/Dash")]
public class DashAbilityDefinition : AbilityDefinition
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private int maxCharges = 2;
    [SerializeField] private float chargeRegenTime = 5f;

    public float DashDistance => dashDistance;
    public float DashDuration => dashDuration;
    public int MaxCharges => maxCharges;
    public float ChargeRegenTime => chargeRegenTime;

    public override AbilityInstance CreateInstance(UnitController controller)
    {
        return new DashAbilityInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of dash ability.
/// Manages charge system and dash execution.
/// </summary>
[System.Serializable]
public class DashAbilityInstance : AbilityInstance
{
    [SerializeField] private int currentCharges;
    [SerializeField] private float chargeRegenTimer;
    [SerializeField] private bool isDashing;

    public DashAbilityDefinition DashDefinition => definition as DashAbilityDefinition;
    public int CurrentCharges => currentCharges;
    public float ChargeRegenTimer => chargeRegenTimer;
    public override bool CanActivate
    {
        get
        {
            Debug.Log($"Checking CanActivate for Dash: Charges={currentCharges}, IsDashing={isDashing}");
            return currentCharges > 0 && !isDashing;
        }
    }

    public DashAbilityInstance(DashAbilityDefinition definition, UnitController controller)
        : base(definition, controller)
    {
        currentCharges = definition.MaxCharges;
        chargeRegenTimer = 0f;
        isDashing = false;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Regenerate charges
        if (currentCharges < DashDefinition.MaxCharges)
        {
            chargeRegenTimer += deltaTime;
            if (chargeRegenTimer >= DashDefinition.ChargeRegenTime)
            {
                currentCharges++;
                chargeRegenTimer = 0f;
                Debug.Log($"{unit.DisplayName} dash charge regenerated ({currentCharges}/{DashDefinition.MaxCharges})");
            }
        }
        else
        {
            chargeRegenTimer = 0f;
        }
    }

    protected override void ActivateAbility()
    {
        if (!CanActivate) return;

        currentCharges--;
        isDashing = true;

        // Get dash direction from controller's forward or movement direction
        Vector3 dashDirection = controller.RenderRoot != null ? controller.RenderRoot.forward : controller.transform.forward;
        Vector3 dashTarget = controller.transform.position + dashDirection.normalized * DashDefinition.DashDistance;

        // Execute dash (controller would handle the actual movement)
        controller.StartCoroutine(DashCoroutine(dashTarget));

        Debug.Log($"{unit.DisplayName} dashed! Charges remaining: {currentCharges}/{DashDefinition.MaxCharges}");
    }

    private System.Collections.IEnumerator DashCoroutine(Vector3 target)
    {
        Vector3 startPos = controller.transform.position;
        float elapsed = 0f;

        var agent = controller.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;

        while (elapsed < DashDefinition.DashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / DashDefinition.DashDuration;
            controller.transform.position = Vector3.Lerp(startPos, target, t);
            yield return null;
        }

        if (agent)
        {
            agent.enabled = true;
            controller.Destination.SetDestination(controller.transform.position);
        }

        isDashing = false;
    }
}
