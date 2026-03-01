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
    [SerializeField] private NavMeshAreaConfig navMeshAreaConfig;
    [SerializeField] private SubmergedComponentDefinition submergedComponentDefinition;

    public float DiveDistance => diveDistance;
    public float DiveHeight => diveHeight;
    public float DiveDuration => diveDuration;
    public float Cooldown => cooldown;
    public NavMeshAreaConfig NavMeshAreaConfig => navMeshAreaConfig;
    public SubmergedComponentDefinition SubmergedComponentDefinition => submergedComponentDefinition;

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
    [SerializeField] private Vector3 directionalInfluence;

    public DiveAbilityDefinition DiveDefinition => definition as DiveAbilityDefinition;
    public override bool CanActivate => base.CanActivate && !isDiving;
    public override bool IsControllingMovement => isDiving;

    public DiveAbilityInstance(DiveAbilityDefinition definition, UnitController controller)
        : base(definition, controller)
    {
        isDiving = false;
        directionalInfluence = Vector3.zero;
    }

    /// <summary>
    /// Apply directional input to the dive trajectory.
    /// </summary>
    public override void ApplyDirectionalInput(Vector3 direction)
    {
        directionalInfluence = direction;
    }

    protected override void ActivateAbility()
    {
        if (!CanActivate) return;

        isDiving = true;
        cooldownRemaining = DiveDefinition.Cooldown;

        // Check if unit has SubmergedComponent and is submerged
        var submergedComponent = controller.GetComponentInstance<SubmergedComponentInstance>();
        if (submergedComponent != null && submergedComponent.IsSubmerged)
        {
            controller.StartCoroutine(ResurfaceCoroutine(submergedComponent));
            Debug.Log($"{unit.DisplayName} is resurfacing!");
        }
        else
        {
            // Initial dive haptic (medium impact for jump)
            TriggerDiveJumpHaptic();

            // Start dive coroutine
            controller.StartCoroutine(DiveCoroutine());
            Debug.Log($"{unit.DisplayName} is diving!");
        }
    }

    private System.Collections.IEnumerator ResurfaceCoroutine(SubmergedComponentInstance submergedComponent)
    {
        var agent = controller.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;

        // Get surface position (add offset magnitude to current position)
        Vector3 submergedPosition = controller.transform.position;
        Vector3 surfacePosition = submergedPosition + Vector3.up * Mathf.Abs(submergedComponent.SubmergedDefinition.UnderwaterOffset);

        // Animate rising to surface
        float submergeTime = 0.3f;
        float elapsed = 0f;
        float underwaterOffset = submergedComponent.SubmergedDefinition.UnderwaterOffset;

        while (elapsed < submergeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / submergeTime;

            // Lerp transform position from submerged to surface
            controller.transform.position = Vector3.Lerp(submergedPosition, surfacePosition, t);

            float currentOffset = Mathf.Lerp(underwaterOffset, 0f, t);
            controller.Destination.SetYOffset(currentOffset);
            yield return null;
        }

        controller.transform.position = surfacePosition;

        // Re-enable agent before calling Resurface (which uses it)
        if (agent != null)
        {
            agent.enabled = true;
        }

        // Use SubmergedComponent to handle resurfacing
        submergedComponent.Resurface(surfacePosition);

        isDiving = false;
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

        // Properly position on NavMesh maintaining correct height
        if (agent)
        {
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(target, out navHit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.Warp(navHit.position);  // Warp handles multi-level NavMesh correctly
            }
            agent.enabled = true;
            controller.Destination.SetDestination(agent.transform.position);
        }

        // If landed in water, submerge and stay submerged
        if (landedInWater)
        {
            Vector3 surfacePosition = controller.transform.position;
            Vector3 submergedPosition = surfacePosition - Vector3.up * 1.5f;

            // Get or add SubmergedComponent
            var submergedComponent = controller.GetComponentInstance<SubmergedComponentInstance>();
            if (submergedComponent == null && DiveDefinition.SubmergedComponentDefinition != null)
            {
                // Add component dynamically
                controller.AddComponent(DiveDefinition.SubmergedComponentDefinition);
                submergedComponent = controller.GetComponentInstance<SubmergedComponentInstance>();
            }

            if (submergedComponent != null)
            {
                // Animate submerging
                float submergeTime = 0.3f;
                float submergeElapsed = 0f;
                while (submergeElapsed < submergeTime)
                {
                    submergeElapsed += Time.deltaTime;
                    controller.transform.position = Vector3.Lerp(surfacePosition, submergedPosition, submergeElapsed / submergeTime);
                    yield return null;
                }

                controller.transform.position = submergedPosition;

                // Use SubmergedComponent to handle underwater state
                submergedComponent.Submerge(surfacePosition);
            }
        }

        directionalInfluence = Vector3.zero; // Reset for next dive
        isDiving = false;
    }

    /// <summary>
    /// Trigger haptic for initial dive jump
    /// </summary>
    private void TriggerDiveJumpHaptic()
    {
        if (iOSHaptics.IsSupported)
        {
            iOSHaptics.Trigger(iOSHaptics.HapticStyle.Medium, 0.8f);
        }
    }
}
