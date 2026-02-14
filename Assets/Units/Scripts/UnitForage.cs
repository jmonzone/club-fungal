using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class UnitForage : UnitBehaviour
{
    [Header("References")]
    [SerializeField] private SporeReference sporeReference;

    [Header("Runtime")]
    [SerializeField] private SporeController targetSpore;

    private NavMeshAgent agent;

    public SporeController TargetSpore => targetSpore;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        Controller.OnBehaviourChanged += OnBehaviourChanged;
    }

    private void OnBehaviourChanged()
    {
        // If no longer in default behaviour and we have a target, request to start foraging
        if (targetSpore && Controller.IsDefaultBehaviour)
        {
            InvokeOnBehaviourRequest();
        }
    }

    public void SetTargetSpore(SporeController spore)
    {
        bool hadTarget = targetSpore != null;
        targetSpore = spore;

        // If we got a new target and we're in default behaviour, request to start foraging
        if (targetSpore != null && Controller.IsDefaultBehaviour)
        {
            InvokeOnBehaviourRequest();
        }
        // If we lost our target while foraging, stop the behaviour
        else if (targetSpore == null && !Controller.IsDefaultBehaviour && IsActive)
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

}
