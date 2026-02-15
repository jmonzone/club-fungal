using UnityEngine;
using UnityEngine.AI;

public class UnitReturnToParty : UnitBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Vector3 returnPosition;

    protected override void Awake()
    {
        base.Awake();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void SetReturnPosition(Vector3 position, bool shouldTeleport = false)
    {
        returnPosition = position;

        if (shouldTeleport)
        {
            // Teleport immediately
            Controller.Teleport(returnPosition, Controller.transform.parent);
        }
        else
        {
            // Walk back by requesting behavior
            InvokeOnBehaviourRequest();
        }
    }

    protected override void OnBehaviourStart()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!IsActive) return;

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            Controller.SetLookPosition(returnPosition);
            Controller.Destination.SetDestination(returnPosition);

            // Check if we've reached the destination
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                StopBehaviour();
            }
        }
    }

    public override void StopBehaviour()
    {
        base.StopBehaviour();
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
        }
    }

    public override int GetPriority()
    {
        // Return to party is triggered externally via SetReturnPosition
        // Priority system doesn't apply - behavior is invoked directly when needed
        return 0;
    }
}
