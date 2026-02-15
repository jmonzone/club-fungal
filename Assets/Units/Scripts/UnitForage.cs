using System;
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
    }

    public void SetTargetSpore(SporeController spore)
    {
        targetSpore = spore;
        Debug.Log($"Unit {name} assigned to spore {spore?.name ?? "null"}");
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
        // High priority if we have a target spore
        return targetSpore != null ? 100 : 0;
    }

}
