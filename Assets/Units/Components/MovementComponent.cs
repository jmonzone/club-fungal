using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ScriptableObject definition for a movement component.
/// Define movement parameters like speed, acceleration, etc.
/// </summary>
[CreateAssetMenu(fileName = "MovementComponent", menuName = "Club Fungal/Units/Components/Movement Component")]
public class MovementComponentDefinition : UnitComponentDefinition
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 3.5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float angularSpeed = 120f;
    [SerializeField] private float stoppingDistance = 0.5f;

    public float BaseSpeed => baseSpeed;
    public float Acceleration => acceleration;
    public float AngularSpeed => angularSpeed;
    public float StoppingDistance => stoppingDistance;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new MovementComponentInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of movement component.
/// Applies movement parameters to NavMeshAgent if present.
/// </summary>
[System.Serializable]
public class MovementComponentInstance : UnitComponentInstance
{
    [SerializeField] private float currentSpeedMultiplier = 1f;
    private NavMeshAgent navMeshAgent;

    public MovementComponentDefinition MovementDefinition => definition as MovementComponentDefinition;
    public float CurrentSpeed => MovementDefinition.BaseSpeed * currentSpeedMultiplier;
    public float SpeedMultiplier => currentSpeedMultiplier;

    public MovementComponentInstance(MovementComponentDefinition definition, UnitController controller) 
        : base(definition, controller)
    {
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        
        // Find NavMeshAgent if it exists
        navMeshAgent = controller.GetComponent<NavMeshAgent>();
        ApplyMovementSettings();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        
        // Keep NavMeshAgent speed synced if speed multiplier changes
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = CurrentSpeed;
        }
    }

    private void ApplyMovementSettings()
    {
        if (navMeshAgent == null) return;

        navMeshAgent.speed = CurrentSpeed;
        navMeshAgent.acceleration = MovementDefinition.Acceleration;
        navMeshAgent.angularSpeed = MovementDefinition.AngularSpeed;
        navMeshAgent.stoppingDistance = MovementDefinition.StoppingDistance;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        currentSpeedMultiplier = Mathf.Max(0f, multiplier);
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = CurrentSpeed;
        }
    }

    public void ResetSpeed()
    {
        currentSpeedMultiplier = 1f;
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = CurrentSpeed;
        }
    }
}
