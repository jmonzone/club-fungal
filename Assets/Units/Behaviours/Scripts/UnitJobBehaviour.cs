using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Abstract base class for stationary job behaviours (e.g. cooking, crafting).
/// Handles NavMesh stop/resume and tracks whether a job is currently assigned.
/// Subclasses implement OnAssign/OnUnassign for job-specific logic.
/// </summary>
public abstract class UnitJobBehaviour : UnitBehaviour
{
    private NavMeshAgent navMeshAgent;
    protected Vector3 jobPosition;

    public bool IsAssigned { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Assign this unit to a stationary job at the given world position.
    /// The priority system will activate this behaviour on the next frame.
    /// </summary>
    public void Assign(Vector3 position)
    {
        jobPosition = position;
        IsAssigned = true;
        OnAssign(position);
    }

    /// <summary>
    /// Remove the stationary job assignment.
    /// The priority system will deactivate this behaviour and restore normal movement.
    /// </summary>
    public void Unassign()
    {
        IsAssigned = false;
        jobPosition = Vector3.zero;
        OnUnassign();
    }

    protected virtual void OnAssign(Vector3 position) { }
    protected virtual void OnUnassign() { }

    protected override void OnBehaviourStart()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
        }

        Controller.SetLookPosition(jobPosition);
    }

    protected override void OnBehaviourStop()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
        }
    }
}
