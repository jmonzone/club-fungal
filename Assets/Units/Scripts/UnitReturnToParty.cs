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

    public void SetReturnPosition(Vector3 position)
    {
        returnPosition = position;

        if (Controller.IsDefaultBehaviour)
        {
            InvokeOnBehaviourRequest();
        }
    }

    protected override void OnBehaviourStart()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(returnPosition);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!IsActive) return;

        if (navMeshAgent != null)
        {
            Controller.SetLookPosition(returnPosition);

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
}
