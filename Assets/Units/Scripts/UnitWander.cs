using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitWander : UnitBehaviour
{
    [Header("Settings")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float minIdleTime = 1f;
    [SerializeField] private float maxIdleTime = 4f;

    private NavMeshAgent navMeshAgent;
    private UnitSpeedModifier speedModifier;
    private float baseSpeed;

    protected override void Awake()
    {
        base.Awake();

        navMeshAgent = GetComponent<NavMeshAgent>();
        speedModifier = GetComponent<UnitSpeedModifier>();
    }

    protected override void OnBehaviourStart()
    {
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            baseSpeed = navMeshAgent.speed;
            StartWander();
        }
        else
        {
            StartCoroutine(WaitForNavMesh());
        }
    }

    private IEnumerator WaitForNavMesh()
    {
        yield return new WaitUntil(() => navMeshAgent != null && navMeshAgent.isOnNavMesh);
        baseSpeed = navMeshAgent.speed;
        StartWander();
    }

    protected override void OnBehaviourStop()
    {
        base.OnBehaviourStop();
        StopWander();
    }

    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
        }
        StopWander();
    }

    public override void UnpauseBehaviour()
    {
        base.UnpauseBehaviour();
        StartWander();
    }

    private void StartWander()
    {
        StartCoroutine(WanderRoutine());
    }

    private void StopWander()
    {
        StopAllCoroutines();
    }

    private IEnumerator WanderRoutine()
    {
        yield return new WaitUntil(() => navMeshAgent.isOnNavMesh);

        navMeshAgent.stoppingDistance = 0.1f;
        navMeshAgent.isStopped = false;

        while (true)
        {
            yield return new WaitUntil(() => navMeshAgent.isOnNavMesh);

            // Get speed multiplier from UnitSpeedModifier
            float speedMultiplier = speedModifier != null ? speedModifier.CurrentSpeedMultiplier : 1f;

            // Scale idle time inversely with speed (faster = less idle time)
            float scaledMinIdleTime = minIdleTime / speedMultiplier;
            float scaledMaxIdleTime = maxIdleTime / speedMultiplier;

            var idleTargetTime = Random.Range(scaledMinIdleTime, scaledMaxIdleTime);
            navMeshAgent.isStopped = true;
            yield return new WaitForSeconds(idleTargetTime);

            yield return new WaitUntil(() => navMeshAgent.isOnNavMesh);

            navMeshAgent.isStopped = false;

            var targetPosition = GetReachableRandomDestination(transform.position, wanderRadius, NavMesh.AllAreas);

            yield return new WaitUntil(() => navMeshAgent.isOnNavMesh);

            navMeshAgent.SetDestination(targetPosition);

            while (navMeshAgent.isOnNavMesh && (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance))
            {
                Controller.SetLookPosition(targetPosition);
                yield return null;
            }
        }
    }

    private Vector3 GetReachableRandomDestination(Vector3 origin, float radius, int layermask, int maxAttempts = 10)
    {
        // assume center of the NavMesh is Vector3.zero (or origin of surface)
        // Vector3 navMeshCenter = Vector3.back * 4f;

        for (int i = 0; i < maxAttempts; i++)
        {
            // 1. Pick a random direction + radius (nonlinear to favor inner area)
            Vector3 randomDirection = Random.onUnitSphere;
            float biasedRadius = Mathf.Pow(Random.value, 2f) * radius; // square favors inner points
            Vector3 randomPoint = origin + randomDirection * biasedRadius;

            // 3. Apply gravitation (optional extra pull)
            // randomPoint = Vector3.Lerp(randomPoint, navMeshCenter, gravitateStrength);

            // 4. Project onto NavMesh
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, layermask))
            {
                // 5. Validate path
                NavMeshPath path = new NavMeshPath();
                if (navMeshAgent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
            }
        }

        // Fallback
        return origin;
    }

}
