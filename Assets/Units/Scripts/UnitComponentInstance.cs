using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base runtime instance class for unit components.
/// Similar to SkillInstance, this wraps a UnitComponentDefinition and manages runtime state.
/// </summary>
[System.Serializable]
public abstract class UnitComponentInstance
{
    [SerializeField] protected UnitComponentDefinition definition;
    [SerializeField] protected UnitController controller;

    public UnitComponentDefinition Definition => definition;
    public UnitController Controller => controller;
    public UnitInstance UnitInstance => controller.Instance;

    public UnitComponentInstance(UnitComponentDefinition definition, UnitController controller)
    {
        this.definition = definition;
        this.controller = controller;
    }

    /// <summary>
    /// Called when the component is first added to a unit.
    /// </summary>
    public virtual void OnInitialize()
    {
    }

    /// <summary>
    /// Called every frame if the component needs updates.
    /// </summary>
    public virtual void OnUpdate()
    {
    }

    /// <summary>
    /// Called when the component is removed or unit is destroyed.
    /// </summary>
    public virtual void OnDestroy()
    {
    }

    /// <summary>
    /// Called when the component should be enabled.
    /// </summary>
    public virtual void OnEnable()
    {
    }

    /// <summary>
    /// Called when the component should be disabled.
    /// </summary>
    public virtual void OnDisable()
    {
    }
}
