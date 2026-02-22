using UnityEngine;

/// <summary>
/// Base ScriptableObject that defines a unit component.
/// Similar to how Skill defines skill data, this defines component data.
/// </summary>
public abstract class UnitComponentDefinition : GURUObject
{
    [SerializeField] private string displayName;
    [TextArea(3, 5)]
    [SerializeField] private string description;

    public string DisplayName => displayName;
    public string Description => description;

    /// <summary>
    /// Factory method to create a component instance from this definition.
    /// Override in derived classes to return specific instance types.
    /// </summary>
    public abstract UnitComponentInstance CreateInstance(UnitController controller);
}
