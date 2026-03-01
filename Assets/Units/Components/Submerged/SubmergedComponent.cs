using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Component that manages underwater/submerged state for units.
/// </summary>
[CreateAssetMenu(fileName = "SubmergedComponent", menuName = "Club Fungal/Units/Components/Submerged Component")]
public class SubmergedComponentDefinition : UnitComponentDefinition
{
    [Header("Submerged Settings")]
    [SerializeField] private float underwaterOffset = -1.5f;
    [SerializeField] private float underwaterSpeedModifier = 0.5f;
    [SerializeField] private NavMeshAreaConfig navMeshAreaConfig;
    [SerializeField] private bool useWaterShadow = true;
    [SerializeField] private GameObject waterShadowPrefab;
    [SerializeField] private bool startSubmerged = false;

    public float UnderwaterOffset => underwaterOffset;
    public float UnderwaterSpeedModifier => underwaterSpeedModifier;
    public NavMeshAreaConfig NavMeshAreaConfig => navMeshAreaConfig;
    public bool UseWaterShadow => useWaterShadow;
    public GameObject WaterShadowPrefab => waterShadowPrefab;
    public bool StartSubmerged => startSubmerged;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new SubmergedComponentInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance managing underwater state.
/// </summary>
[System.Serializable]
public class SubmergedComponentInstance : UnitComponentInstance
{
    [SerializeField] private bool isSubmerged;
    private GameObject waterShadowInstance;
    private int originalAreaMask;
    private float underwaterHapticTimer;
    private const float UnderwaterHapticInterval = 0.6f;

    public SubmergedComponentDefinition SubmergedDefinition => definition as SubmergedComponentDefinition;
    public bool IsSubmerged => isSubmerged;

    public SubmergedComponentInstance(SubmergedComponentDefinition definition, UnitController controller)
        : base(definition, controller)
    {
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        // Start submerged if configured (for fish/aquatic units)
        if (SubmergedDefinition.StartSubmerged)
        {
            Submerge(controller.transform.position, skipAnimation: true);
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Update water shadow position to follow unit horizontally
        if (isSubmerged && waterShadowInstance != null && controller != null)
        {
            Vector3 shadowPos = controller.transform.position;
            shadowPos.y = waterShadowInstance.transform.position.y;
            waterShadowInstance.transform.position = shadowPos;

            // Trigger periodic underwater haptics
            underwaterHapticTimer += Time.deltaTime;
            if (underwaterHapticTimer >= UnderwaterHapticInterval)
            {
                TriggerUnderwaterSwimHaptic();
                underwaterHapticTimer = 0f;
            }
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();

        // Hide water shadow when unit is disabled
        if (waterShadowInstance != null)
        {
            waterShadowInstance.SetActive(false);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();

        // Show water shadow when unit is re-enabled
        if (waterShadowInstance != null)
        {
            waterShadowInstance.SetActive(true);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (waterShadowInstance != null)
        {
            Object.Destroy(waterShadowInstance);
            waterShadowInstance = null;
        }
    }

    /// <summary>
    /// Submerge the unit underwater.
    /// </summary>
    public void Submerge(Vector3 surfacePosition, bool skipAnimation = false)
    {
        if (isSubmerged) return;

        isSubmerged = true;
        underwaterHapticTimer = 0f;

        var agent = controller.GetComponent<NavMeshAgent>();
        var speedModifier = controller.GetComponent<UnitSpeedModifier>();

        // Set underwater offset
        controller.Destination.SetYOffset(SubmergedDefinition.UnderwaterOffset);

        // Apply underwater speed modifier
        if (speedModifier != null)
        {
            speedModifier.AddSpeedModifier(SubmergedDefinition.UnderwaterSpeedModifier);
        }

        // Constrain navigation to water terrain only
        if (agent != null && SubmergedDefinition.NavMeshAreaConfig != null)
        {
            originalAreaMask = agent.areaMask;
            agent.areaMask = SubmergedDefinition.NavMeshAreaConfig.WaterTerrainAreaMask;
        }

        // Create water shadow at surface position
        if (SubmergedDefinition.UseWaterShadow && SubmergedDefinition.WaterShadowPrefab != null)
        {
            waterShadowInstance = Object.Instantiate(SubmergedDefinition.WaterShadowPrefab, surfacePosition, Quaternion.identity);
        }

        if (!skipAnimation)
        {
            TriggerWaterEntryHaptic();
        }
    }

    /// <summary>
    /// Resurface the unit from underwater.
    /// </summary>
    public void Resurface(Vector3 surfacePosition)
    {
        if (!isSubmerged) return;

        isSubmerged = false;
        underwaterHapticTimer = 0f;

        var agent = controller.GetComponent<NavMeshAgent>();
        var speedModifier = controller.GetComponent<UnitSpeedModifier>();

        // Remove underwater offset
        controller.Destination.SetYOffset(0f);

        // Remove underwater speed modifier
        if (speedModifier != null)
        {
            speedModifier.RemoveSpeedModifier(SubmergedDefinition.UnderwaterSpeedModifier);
        }

        // Restore original area mask
        if (agent != null)
        {
            agent.areaMask = originalAreaMask;

            // Sample NavMesh to find correct surface position
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(surfacePosition, out navHit, 10f, agent.areaMask))
            {
                agent.Warp(navHit.position);
            }
            else
            {
                agent.Warp(surfacePosition);
            }

            controller.Destination.SetDestination(agent.transform.position);
        }

        // Destroy water shadow
        if (waterShadowInstance != null)
        {
            Object.Destroy(waterShadowInstance);
            waterShadowInstance = null;
        }

        TriggerResurfaceHaptic();
    }

    private void TriggerWaterEntryHaptic()
    {
        if (iOSHaptics.IsSupported)
        {
            iOSHaptics.Trigger(iOSHaptics.HapticStyle.Heavy, 1f);
        }
    }

    private void TriggerUnderwaterSwimHaptic()
    {
        if (iOSHaptics.IsSupported)
        {
            iOSHaptics.Trigger(iOSHaptics.HapticStyle.Heavy, 0.6f);
        }
    }

    private void TriggerResurfaceHaptic()
    {
        if (iOSHaptics.IsSupported)
        {
            iOSHaptics.Trigger(iOSHaptics.HapticStyle.Soft, 0.7f);
        }
    }
}
