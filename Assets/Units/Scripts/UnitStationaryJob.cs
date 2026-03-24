using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Behaviour that pins a unit to a stationary job position.
/// While active: disables NavMesh movement and keeps the unit at the assigned position.
/// Assign a job via Assign(), remove it via Unassign().
/// Priority system activates/deactivates this behaviour automatically based on HasJob.
/// </summary>
public class UnitStationaryJob : UnitBehaviour
{
    private NavMeshAgent navMeshAgent;
    private ActivityReference assignedActivity;
    private Vector3 jobPosition;

    public ActivityReference AssignedActivity => assignedActivity;
    public bool HasJob => assignedActivity != null;

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Assign this unit to a stationary job at the given world position.
    /// The priority system will activate this behaviour on the next frame.
    /// </summary>
    public void Assign(ActivityReference activity, Vector3 position)
    {
        assignedActivity = activity;
        jobPosition = position;
    }

    /// <summary>
    /// Remove the stationary job assignment.
    /// The priority system will deactivate this behaviour and restore normal movement.
    /// </summary>
    public void Unassign()
    {
        assignedActivity = null;
        jobPosition = Vector3.zero;
    }

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
