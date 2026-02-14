using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class UnitForage : UnitBehaviour
{
    [Header("References")]
    [SerializeField] private SporeReference sporeReference;
    [SerializeField] private UnitForageService forageService;

    [Header("Runtime")]
    [SerializeField] private SporeController targetSpore;

    private NavMeshAgent agent;

    public SporeController TargetSpore => targetSpore;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    public void SetTargetSpore(SporeController spore)
    {
        targetSpore = spore;

        // If we got a new target, request to start foraging
        if (targetSpore != null)
        {
            InvokeOnBehaviourRequest();
        }
        // If we lost our target while foraging, stop the behaviour
        else if (IsActive)
        {
            StopBehaviour();
        }
    }


    protected override void OnBehaviourStart()
    {
        if (targetSpore)
        {
            agent.isStopped = false;
            StartCoroutine(ForagingBehaviour());
        }
    }

    private IEnumerator ForagingBehaviour()
    {
        while (targetSpore)
        {
            agent.SetDestination(targetSpore.transform.position);
            Controller.SetLookPosition(targetSpore.transform.position);
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
        // If we don't have a target, request assignment from service
        if (targetSpore == null && forageService != null)
        {
            forageService.RequestAssignment(this);
        }

        // High priority if we have a target spore
        return targetSpore != null ? 100 : 0;
    }

}
