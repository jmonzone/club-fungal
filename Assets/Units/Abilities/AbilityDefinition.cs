using UnityEngine;

/// <summary>
/// Base ScriptableObject definition for unit abilities.
/// Defines the configuration and creates runtime instances.
/// </summary>
public abstract class AbilityDefinition : GURUObject
{
    [Header("Ability Info")]
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField][TextArea] private string description;

    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string Description => description;

    /// <summary>
    /// Create a runtime instance of this ability for a specific unit.
    /// </summary>
    public abstract AbilityInstance CreateInstance(UnitController controller);
}
