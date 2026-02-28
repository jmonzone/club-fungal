using UnityEngine;

/// <summary>
/// Ability that allows units to toggle between ground and air movement.
/// </summary>
[CreateAssetMenu(fileName = "FlyAbility", menuName = "Club Fungal/Abilities/Fly")]
public class FlyAbilityDefinition : AbilityDefinition
{
    [Header("Fly Settings")]
    [SerializeField] private float airHeight = 3f;
    [SerializeField] private float transitionSpeed = 2f;
    [SerializeField] private float flySpeedBonus = 1.5f;

    public float AirHeight => airHeight;
    public float TransitionSpeed => transitionSpeed;
    public float FlySpeedBonus => flySpeedBonus;

    public override AbilityInstance CreateInstance(UnitController controller)
    {
        return new FlyAbilityInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of fly ability.
/// Toggles between ground and air movement modes.
/// </summary>
[System.Serializable]
public class FlyAbilityInstance : AbilityInstance
{
    [SerializeField] private bool isFlying;
    [SerializeField] private bool isTransitioning;

    public FlyAbilityDefinition FlyDefinition => definition as FlyAbilityDefinition;
    public bool IsFlying => isFlying;
    public override bool CanActivate => base.CanActivate && !isTransitioning;

    public FlyAbilityInstance(FlyAbilityDefinition definition, UnitController controller)
        : base(definition, controller)
    {
        isFlying = false;
        isTransitioning = false;
    }

    protected override void ActivateAbility()
    {
        if (!CanActivate) return;

        // Toggle flying state
        if (isFlying)
        {
            // Landing
            controller.StartCoroutine(LandCoroutine());
            Debug.Log($"{unit.DisplayName} is landing!");
        }
        else
        {
            // Taking off
            TriggerTakeoffHaptic();
            controller.StartCoroutine(TakeoffCoroutine());
            Debug.Log($"{unit.DisplayName} is taking off!");
        }
    }

    private System.Collections.IEnumerator TakeoffCoroutine()
    {
        isTransitioning = true;
        Vector3 startPos = controller.transform.position;
        float targetHeight = FlyDefinition.AirHeight;
        float elapsed = 0f;
        float duration = 1f / FlyDefinition.TransitionSpeed;

        // Gradually increase Y offset
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentHeight = Mathf.Lerp(0f, targetHeight, t);
            controller.Destination.SetYOffset(currentHeight);
            yield return null;
        }

        controller.Destination.SetYOffset(targetHeight);
        isFlying = true;
        isActive = true;
        isTransitioning = false;

        // Apply speed bonus and terrain immunity
        var speedModifier = controller.GetComponent<UnitSpeedModifier>();
        if (speedModifier != null)
        {
            speedModifier.AddSpeedModifier(FlyDefinition.FlySpeedBonus);
            speedModifier.SetTerrainImmunity(true);
        }

        Debug.Log($"{unit.DisplayName} is now flying at height {targetHeight}!");
    }

    private System.Collections.IEnumerator LandCoroutine()
    {
        isTransitioning = true;
        float startHeight = FlyDefinition.AirHeight;
        float elapsed = 0f;
        float duration = 1f / FlyDefinition.TransitionSpeed;

        // Remove speed bonus and terrain immunity immediately
        var speedModifier = controller.GetComponent<UnitSpeedModifier>();
        if (speedModifier != null)
        {
            speedModifier.RemoveSpeedModifier(FlyDefinition.FlySpeedBonus);
            speedModifier.SetTerrainImmunity(false);
        }

        // Gradually decrease Y offset
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentHeight = Mathf.Lerp(startHeight, 0f, t);
            controller.Destination.SetYOffset(currentHeight);
            yield return null;
        }

        controller.Destination.SetYOffset(0f);
        isFlying = false;
        isActive = false;
        isTransitioning = false;

        TriggerLandingHaptic();
        Debug.Log($"{unit.DisplayName} has landed!");
    }

    public override void Deactivate()
    {
        if (isFlying)
        {
            // Force immediate landing
            var speedModifier = controller.GetComponent<UnitSpeedModifier>();
            if (speedModifier != null)
            {
                speedModifier.RemoveSpeedModifier(FlyDefinition.FlySpeedBonus);
                speedModifier.SetTerrainImmunity(false);
            }
            controller.Destination.SetYOffset(0f);
        }

        isFlying = false;
        isTransitioning = false;
        base.Deactivate();
    }

    /// <summary>
    /// Trigger haptic for takeoff
    /// </summary>
    private void TriggerTakeoffHaptic()
    {
        if (iOSHaptics.IsSupported)
        {
            iOSHaptics.Trigger(iOSHaptics.HapticStyle.Medium, 0.9f);
        }
    }

    /// <summary>
    /// Trigger haptic for landing
    /// </summary>
    private void TriggerLandingHaptic()
    {
        if (iOSHaptics.IsSupported)
        {
            iOSHaptics.Trigger(iOSHaptics.HapticStyle.Medium, 0.7f);
        }
    }
}
