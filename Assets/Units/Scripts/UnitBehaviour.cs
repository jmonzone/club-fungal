using UnityEngine;
using UnityEngine.Events;

public abstract class UnitComponent : MonoBehaviour
{
    public UnitController Controller { get; private set; }
    public UnitInstance Instance => Controller.Instance;

    protected virtual void Awake()
    {
        Controller = GetComponentInParent<UnitController>();
        Controller.OnInitialized += OnInitialized;
    }

    protected virtual void OnInitialized()
    {
    }
}

public abstract class UnitBehaviour : UnitComponent
{
    [SerializeField] private bool isActive;
    public bool IsActive => isActive;

    [Header("Behaviour Conditions")]
    [SerializeField] private BehaviourPriorityModifier[] priorityModifiers;

    /// <summary>
    /// Calculates total priority modifier from all assigned modifiers.
    /// Returns int.MinValue if any modifier blocks the behavior.
    /// </summary>
    private int GetTotalPriorityModifier()
    {
        if (priorityModifiers == null || priorityModifiers.Length == 0)
            return 0;

        int total = 0;
        foreach (var modifier in priorityModifiers)
        {
            if (modifier == null) continue;

            int modifierValue = modifier.GetPriorityModifier();

            // If any modifier blocks, behavior is blocked
            if (modifierValue == int.MinValue)
                return int.MinValue;

            total += modifierValue;
        }
        return total;
    }

    protected virtual void Update()
    {

    }

    public virtual void StartBehaviour()
    {
        if (!isActive)
        {
            isActive = true;
            OnBehaviourStart();
        }
    }

    protected virtual void OnBehaviourStart()
    {
    }

    public virtual void StopBehaviour()
    {
        if (isActive)
        {
            isActive = false;
            OnBehaviourStop();
        }
    }

    protected virtual void OnBehaviourStop()
    {
    }

    public virtual void PauseBehaviour()
    {
    }

    public virtual void UnpauseBehaviour()
    {
    }

    /// <summary>
    /// Returns the priority of this behavior. Higher values = higher priority.
    /// Return 0 if this behavior should not be active.
    /// Priority modifiers are automatically applied.
    /// </summary>
    public virtual int GetPriority()
    {
        // Get base priority from derived class
        int basePriority = GetBasePriority();

        // Apply modifiers
        int modifier = GetTotalPriorityModifier();

        // If any modifier blocks, return 0
        if (modifier == int.MinValue)
            return 0;

        // Apply modifier to base priority
        return Mathf.Max(0, basePriority + modifier);
    }

    /// <summary>
    /// Override this to define base priority logic for your behavior.
    /// This is called before priority modifiers are applied.
    /// </summary>
    protected virtual int GetBasePriority()
    {
        return 0;
    }
}