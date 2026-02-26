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

    public float AirHeight => airHeight;
    public float TransitionSpeed => transitionSpeed;

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
    private bool isFlying;

    public FlyAbilityDefinition FlyDefinition => definition as FlyAbilityDefinition;
    public bool IsFlying => isFlying;

    public FlyAbilityInstance(FlyAbilityDefinition definition, UnitController controller)
        : base(definition, controller)
    {
        isFlying = false;
    }

    protected override void ActivateAbility()
    {
        if (!CanActivate) return;

        // Toggle flying state
        isFlying = !isFlying;
        isActive = isFlying;

        Debug.Log($"{unit.DisplayName} is now {(isFlying ? "flying" : "walking")}");
    }

    public override void Deactivate()
    {
        isFlying = false;
        base.Deactivate();
    }
}
