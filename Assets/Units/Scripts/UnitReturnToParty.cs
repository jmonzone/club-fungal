using UnityEngine;
using UnityEngine.AI;

public class UnitReturnToParty : UnitBehaviour, IReturnPositionable
{
    private NavMeshAgent navMeshAgent;
    private Vector3 returnPosition;
    private const float acceptableDistance = 1f; // Distance considered "close enough" to return position

    public Vector3 ReturnPosition => returnPosition;

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
        // Priority system will automatically activate this behavior if needed
    }

    protected override void OnBehaviourStart()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
        }
    }

    protected override void OnBehaviourStop()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
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
        }
    }
}
