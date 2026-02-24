using System;
using UnityEngine;

/// <summary>
/// Base runtime instance of an ability attached to a unit.
/// Handles cooldowns, charges, and execution logic.
/// </summary>
[Serializable]
public abstract class AbilityInstance
{
    [SerializeField] protected AbilityDefinition definition;
    [SerializeField] protected UnitInstance unit;
    [SerializeField] protected float cooldownRemaining;
    [SerializeField] protected bool isActive;

    public AbilityDefinition Definition => definition;
    public UnitInstance Unit => unit;
    public float CooldownRemaining => cooldownRemaining;
    public bool IsActive => isActive;
    public virtual bool CanActivate => cooldownRemaining <= 0f;

    protected AbilityInstance(AbilityDefinition definition, UnitInstance unit)
    {
        this.definition = definition;
        this.unit = unit;
        this.cooldownRemaining = 0f;
        this.isActive = false;
    }

    /// <summary>
    /// Called every frame to update cooldowns and ability state.
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= deltaTime;
        }
    }

    /// <summary>
    /// Attempt to activate the ability.
    /// </summary>
    public abstract void Activate(UnitController controller);

    /// <summary>
    /// Deactivate the ability if it's a toggle or persistent ability.
    /// </summary>
    public virtual void Deactivate(UnitController controller)
    {
        isActive = false;
    }
}
