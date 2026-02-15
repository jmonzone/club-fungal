using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public interface IForageTarget : ITarget
{
    void OnForaged(UnitController forager);
}

public class UnitForage : UnitBehaviour
{
    private IForageTarget target;
    private NavMeshAgent agent;

    public event UnityAction OnForageTargetReached;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    public void SetTarget(IForageTarget target)
    {
        Debug.Log("Setting forage target: " + target?.Transform.name ?? "None");
        this.target = target;
    }


    protected override void OnBehaviourStart()
    {
        agent.isStopped = false;
        StartCoroutine(ForagingBehaviour());
    }

    private IEnumerator ForagingBehaviour()
    {
        Debug.Log("Starting foraging behavior towards target: " + target.Transform.name);
        while (target != null)
        {
            Controller.Destination.SetDestination(target.Transform.position);
            Controller.SetLookPosition(target.Transform.position);

            if (Vector3.Distance(transform.position, target.Transform.position) <= 2f)
            {
                Debug.Log("Reached forage target: " + target.Transform.name);
                target.OnForaged(Controller);
                OnForageTargetReached?.Invoke();
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f)); // small delay after foraging
            }

            yield return null;
        }

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
        Debug.Log("Checking forage behavior priority. Target: " + (target != null ? target.Transform.name : "None"));
        // High priority if we have a target
        return target != null ? 100 : 0;
    }

}
