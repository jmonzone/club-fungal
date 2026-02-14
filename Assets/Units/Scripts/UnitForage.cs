using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class UnitForage : UnitBehaviour
{
    private NavMeshAgent agent;
    private IForageTarget target;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    public void SetTarget(IForageTarget forageTarget)
    {
        target = forageTarget;
    }


    protected override void OnBehaviourStart()
    {
        if (target != null)
        {
            agent.isStopped = false;
            StartCoroutine(ForagingBehaviour());
        }
    }

    private IEnumerator ForagingBehaviour()
    {
        while (target != null && target.IsAvailable)
        {
            agent.SetDestination(target.Transform.position);
            Controller.SetLookPosition(target.Transform.position);

            // Check if we've reached the target
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                target.OnForaged(Controller);
                target = null;
                break;
            }

            yield return null;
        }

        target = null;

        StopBehaviour();
    }

    public override void StopBehaviour()
    {
        base.StopBehaviour();
        agent.isStopped = true;
        StopAllCoroutines();
    }

    public override int GetPriority()
    {
        // Service assigns targets proactively when behavior changes
        return target != null ? 100 : 0;
    }

}
