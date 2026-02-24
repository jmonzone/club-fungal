using UnityEngine;

/// <summary>
/// Ability that allows units to dive in and out of water when in water terrain.
/// </summary>
[CreateAssetMenu(fileName = "DiveAbility", menuName = "Club Fungal/Abilities/Dive")]
public class DiveAbilityDefinition : AbilityDefinition
{
    [Header("Dive Settings")]
    [SerializeField] private float underwaterDepth = 2f;
    [SerializeField] private float transitionSpeed = 3f;
    [SerializeField] private float cooldown = 1f;

    public float UnderwaterDepth => underwaterDepth;
    public float TransitionSpeed => transitionSpeed;
    public float Cooldown => cooldown;

    public override AbilityInstance CreateInstance(UnitInstance unit)
    {
        return new DiveAbilityInstance(this, unit);
    }
}

/// <summary>
/// Runtime instance of dive ability.
/// Toggles between surface and underwater when in water terrain.
/// </summary>
[System.Serializable]
public class DiveAbilityInstance : AbilityInstance
{
    private bool isUnderwater;
    private bool isInWater;

    public DiveAbilityDefinition DiveDefinition => definition as DiveAbilityDefinition;
    public bool IsUnderwater => isUnderwater;
    public override bool CanActivate => base.CanActivate && isInWater;

    public DiveAbilityInstance(DiveAbilityDefinition definition, UnitInstance unit)
        : base(definition, unit)
    {
        isUnderwater = false;
        isInWater = false;
    }

    /// <summary>
    /// Set whether the unit is currently in water terrain.
    /// This is typically called by the terrain/environment system.
    /// </summary>
    public void SetInWater(bool inWater)
    {
        isInWater = inWater;
        
        // Force surface when leaving water
        if (!inWater && isUnderwater)
        {
            isUnderwater = false;
            isActive = false;
        }
    }

    public override void Activate(UnitController controller)
    {
        if (!CanActivate) return;

        // Toggle underwater state
        isUnderwater = !isUnderwater;
        isActive = isUnderwater;
        cooldownRemaining = DiveDefinition.Cooldown;

        Debug.Log($"{unit.DisplayName} {(isUnderwater ? "dived underwater" : "surfaced")}");
    }

    public override void Deactivate(UnitController controller)
    {
        isUnderwater = false;
        base.Deactivate(controller);
    }
}
