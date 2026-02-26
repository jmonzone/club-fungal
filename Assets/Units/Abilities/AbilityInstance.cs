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

    protected UnitController controller;

    public AbilityDefinition Definition => definition;
    public UnitInstance Unit => unit;
    public UnitController Controller => controller;
    public float CooldownRemaining => cooldownRemaining;
    public bool IsActive => isActive;
    public virtual bool CanActivate => cooldownRemaining <= 0f;
    public virtual bool IsControllingMovement => false;

    protected AbilityInstance(AbilityDefinition definition, UnitController controller)
    {
        this.definition = definition;
        this.controller = controller;
        this.unit = controller.Instance;
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
    public void Activate()
    {
        ActivateAbility();
    }

    /// <summary>
    /// Internal activation logic to be implemented by subclasses.
    /// </summary>
    protected abstract void ActivateAbility();

    /// <summary>
    /// Deactivate the ability if it's a toggle or persistent ability.
    /// </summary>
    public virtual void Deactivate()
    {
        isActive = false;
    }

    /// <summary>
    /// Apply directional input to the ability if it supports it.
    /// </summary>
    public virtual void ApplyDirectionalInput(Vector3 direction)
    {
        // Base implementation does nothing
    }
}
