using UnityEngine;

/// <summary>
/// Ability that allows units to jump forward in a short arc trajectory.
/// </summary>
[CreateAssetMenu(fileName = "DiveAbility", menuName = "Club Fungal/Abilities/Dive")]
public class DiveAbilityDefinition : AbilityDefinition
{
    [Header("Dive Settings")]
    [SerializeField] private float diveDistance = 3f;
    [SerializeField] private float diveHeight = 2f;
    [SerializeField] private float diveDuration = 0.5f;
    [SerializeField] private float cooldown = 2f;
    [SerializeField] private float underwaterOffset = -1.5f;
    [SerializeField] private NavMeshAreaConfig navMeshAreaConfig;
    [SerializeField] private GameObject waterShadowPrefab;

    public float DiveDistance => diveDistance;
    public float DiveHeight => diveHeight;
    public float DiveDuration => diveDuration;
    public float Cooldown => cooldown;
    public float UnderwaterOffset => underwaterOffset;
    public NavMeshAreaConfig NavMeshAreaConfig => navMeshAreaConfig;
    public GameObject WaterShadowPrefab => waterShadowPrefab;

    public override AbilityInstance CreateInstance(UnitController controller)
    {
        return new DiveAbilityInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of dive ability.
/// Makes the unit jump forward in an arc trajectory.
/// </summary>
[System.Serializable]
public class DiveAbilityInstance : AbilityInstance
{
    [SerializeField] private bool isDiving;
    [SerializeField] private bool isSubmerged;
    [SerializeField] private Vector3 directionalInfluence;
    private GameObject waterShadowInstance;

    public DiveAbilityDefinition DiveDefinition => definition as DiveAbilityDefinition;
    public override bool CanActivate => base.CanActivate && !isDiving;
    public override bool IsControllingMovement => isDiving;

    public DiveAbilityInstance(DiveAbilityDefinition definition, UnitController controller)
        : base(definition, controller)
    {
        isDiving = false;
        isSubmerged = false;
        directionalInfluence = Vector3.zero;
    }

    /// <summary>
    /// Apply directional input to the dive trajectory.
    /// </summary>
    public override void ApplyDirectionalInput(Vector3 direction)
    {
        directionalInfluence = direction;
    }

    /// <summary>
    /// Update water shadow position to follow unit horizontally.
    /// </summary>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        // Keep water shadow at unit's horizontal position but at water surface
        if (isSubmerged && waterShadowInstance != null && controller != null)
        {
            Vector3 shadowPos = controller.transform.position;
            shadowPos.y = waterShadowInstance.transform.position.y; // Keep at original water surface Y
            waterShadowInstance.transform.position = shadowPos;
        }
    }

    /// <summary>
    /// Cleanup water shadow when ability is deactivated.
    /// </summary>
    public override void Deactivate()
    {
        base.Deactivate();

        if (waterShadowInstance != null)
        {
            Object.Destroy(waterShadowInstance);
            waterShadowInstance = null;
        }
    }

    protected override void ActivateAbility()
    {
        if (!CanActivate) return;

        isDiving = true;
        cooldownRemaining = DiveDefinition.Cooldown;

        // If already submerged, resurface instead
        if (isSubmerged)
        {
            controller.StartCoroutine(ResurfaceCoroutine());
            Debug.Log($"{unit.DisplayName} is resurfacing!");
        }
        else
        {
            // Start dive coroutine
            controller.StartCoroutine(DiveCoroutine());
            Debug.Log($"{unit.DisplayName} is diving!");
        }
    }

    private System.Collections.IEnumerator ResurfaceCoroutine()
    {
        Vector3 submergedPosition = controller.transform.position;
        Vector3 surfacePosition = submergedPosition + Vector3.up * 1.5f;

        float submergeTime = 0.3f;
        float elapsed = 0f;
        while (elapsed < submergeTime)
        {
            elapsed += Time.deltaTime;
            controller.transform.position = Vector3.Lerp(submergedPosition, surfacePosition, elapsed / submergeTime);
            yield return null;
        }

        controller.transform.position = surfacePosition;
        isSubmerged = false;
        isDiving = false;

        // Clear underwater offset
        controller.Destination.SetYOffset(0f);

        // Destroy water shadow
        if (waterShadowInstance != null)
        {
            Object.Destroy(waterShadowInstance);
            waterShadowInstance = null;
        }
    }

    private System.Collections.IEnumerator DiveCoroutine()
    {
        Vector3 startPos = controller.transform.position;

        // Wait one frame to let joystick feed directional input
        yield return null;

        // NOW capture the dive direction
        Vector3 diveDirection;

        if (directionalInfluence.sqrMagnitude > 0.01f)
        {
            // Use directional input from joystick
            diveDirection = directionalInfluence.normalized;
            diveDirection.y = 0f;
            Debug.Log($"{unit.DisplayName} diving with joystick direction: {diveDirection}");
        }
        else
        {
            // Fall back to look direction
            diveDirection = controller.RenderRoot != null ? controller.RenderRoot.forward : controller.transform.forward;
            Debug.Log($"{unit.DisplayName} diving with look direction: {diveDirection}");
        }

        Vector3 target = startPos + diveDirection.normalized * DiveDefinition.DiveDistance;
        float elapsed = 0f;

        var agent = controller.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;

        // Calculate dive direction (horizontal only)
        Vector3 horizontalDirection = (target - startPos);
        horizontalDirection.y = 0f;
        horizontalDirection = horizontalDirection.normalized;

        // Set look position ahead and downward during dive
        Vector3 diveLookTarget = target + horizontalDirection * 2f + Vector3.down * 1f;
        controller.SetLookPosition(diveLookTarget);

        while (elapsed < DiveDefinition.DiveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / DiveDefinition.DiveDuration;

            // Apply directional influence to target (only during first half of dive)
            if (t < 0.5f && directionalInfluence.sqrMagnitude > 0.01f)
            {
                target += directionalInfluence * Time.deltaTime * 0.5f;
                target.y = startPos.y; // Keep on same height level
            }

            // Horizontal movement (linear)
            Vector3 horizontalPos = Vector3.Lerp(startPos, target, t);

            // Vertical movement (parabolic arc)
            float heightOffset = DiveDefinition.DiveHeight * Mathf.Sin(t * Mathf.PI);

            // Combine horizontal and vertical
            controller.transform.position = horizontalPos + Vector3.up * heightOffset;

            yield return null;
        }

        // Ensure we end exactly at target position
        controller.transform.position = target;

        // Check if we landed in water (water terrain)
        bool landedInWater = false;
        if (DiveDefinition.NavMeshAreaConfig != null)
        {
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(target, out navHit, 1f, DiveDefinition.NavMeshAreaConfig.WaterTerrainAreaMask))
            {
                landedInWater = true;
                Debug.Log($"{unit.DisplayName} landed in water! Submerging...");
            }
        }

        // Set look position forward after landing
        controller.SetLookPosition(target + horizontalDirection * 2f);

        if (agent)
        {
            agent.enabled = true;
            controller.Destination.SetDestination(controller.transform.position);
        }

        // If landed in water, submerge and stay submerged
        if (landedInWater)
        {
            Vector3 surfacePosition = controller.transform.position;
            Vector3 submergedPosition = surfacePosition - Vector3.up * 1.5f;

            // Submerge
            float submergeTime = 0.3f;
            float submergeElapsed = 0f;
            while (submergeElapsed < submergeTime)
            {
                submergeElapsed += Time.deltaTime;
                controller.transform.position = Vector3.Lerp(surfacePosition, submergedPosition, submergeElapsed / submergeTime);
                yield return null;
            }

            controller.transform.position = submergedPosition;
            isSubmerged = true;

            // Set underwater offset so unit stays at this depth
            controller.Destination.SetYOffset(DiveDefinition.UnderwaterOffset);

            // Instantiate water shadow at surface position
            if (DiveDefinition.WaterShadowPrefab != null)
            {
                waterShadowInstance = Object.Instantiate(DiveDefinition.WaterShadowPrefab, surfacePosition, Quaternion.identity);
            }
        }

        directionalInfluence = Vector3.zero; // Reset for next dive
        isDiving = false;
    }
}
