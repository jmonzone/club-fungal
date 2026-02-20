using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class UnitCombat : UnitBehaviour, IReturnPositionable
{
    [Header("References")]
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitControllerService unitControllerService;

    [Header("Combat Settings")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackRange = 15f;
    [SerializeField] private float detectionRangeOffset = 1f;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float targetSearchInterval = 0.5f;

    private UnitController target;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;
    private float lastAttackTime;
    private float lastTargetSearchTime;
    private ObjectPool<Projectile> projectilePool;

    public UnitController CurrentTarget => target;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (projectileSpawnPoint == null)
        {
            projectileSpawnPoint = transform;
        }

        // Initialize projectile pool
        if (projectilePrefab != null)
        {
            projectilePool = new ObjectPool<Projectile>(projectilePrefab, poolSize, null, projectile =>
            {
                projectile.OnHit += () => projectilePool.Return(projectile);
            });
        }
    }

    public void SetTarget(UnitController newTarget)
    {
        // Unsubscribe from previous target's death event
        if (target != null && target.Death != null)
        {
            target.Death.OnUnitDeath -= OnTargetDied;
        }

        this.target = newTarget;

        // Subscribe to new target's death event
        if (target != null && target.Death != null)
        {
            target.Death.OnUnitDeath += OnTargetDied;
        }
    }

    private void OnTargetDied()
    {
        // Clear target when it dies
        if (target != null && target.Death != null)
        {
            target.Death.OnUnitDeath -= OnTargetDied;
        }
        target = null;
    }

    private void FindNearestTarget()
    {
        if (unitControllerService == null || networkRunService == null || networkRunService.Party == null)
            return;

        UnitController nearestEnemy = null;

        // Detection range is party tether range + offset
        float detectionRange = networkRunService.MaxTetherDistance + detectionRangeOffset;
        float nearestDistance = detectionRange;

        foreach (var controller in unitControllerService.Controllers)
        {
            // Skip self
            if (controller == Controller)
                continue;

            // Only target enemies
            if (!controller.IsEnemy)
                continue;

            // Skip dead units
            if (controller.Death != null && controller.Death.IsDead)
                continue;

            // Check distance
            float distance = Vector3.Distance(transform.position, controller.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = controller;
            }
        }

        SetTarget(nearestEnemy);
    }

    public void SetReturnPosition(Vector3 position, bool shouldTeleport = false)
    {
        // Only handle teleporting for combat units
        if (shouldTeleport)
        {
            Controller.Teleport(position, Controller.transform.parent);
        }
    }

    protected override void OnBehaviourStart()
    {
        if (agent != null)
        {
            agent.isStopped = false;
        }
        isAttacking = false;
        StartCoroutine(CombatBehaviour());
    }

    protected override void OnBehaviourStop()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
        isAttacking = false;
        StopAllCoroutines();
    }

    private IEnumerator CombatBehaviour()
    {
        while (true)
        {
            // Auto-find target periodically
            if (Time.time - lastTargetSearchTime >= targetSearchInterval)
            {
                FindNearestTarget();
                lastTargetSearchTime = Time.time;
            }

            // If no target, just wait
            if (target == null)
            {
                yield return null;
                continue;
            }

            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance <= attackRange)
            {
                // Stop moving and attack
                if (agent != null)
                {
                    agent.isStopped = true;
                }

                // Face the target
                Controller.SetLookPosition(target.transform.position);

                // Attack if enough time has passed
                if (Time.time - lastAttackTime >= attackInterval)
                {
                    ThrowProjectile();
                    lastAttackTime = Time.time;
                }
            }
            else
            {
                // Move toward target
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                    agent.SetDestination(target.transform.position);
                }
                Controller.SetLookPosition(target.transform.position);
            }

            yield return null;
        }
    }

    private void ThrowProjectile()
    {
        if (projectilePool == null)
        {
            Debug.LogWarning("UnitCombat: Projectile pool not initialized");
            return;
        }

        // Trigger attack animation if available
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }

        // Get projectile from pool
        Projectile projectile = projectilePool.Get();
        Vector3 spawnPosition = projectileSpawnPoint.position + Vector3.up * 1f;
        projectile.transform.position = spawnPosition;
        projectile.transform.rotation = Quaternion.identity;
        projectile.Initialize(target, damage);
    }

    protected override int GetBasePriority()
    {
        // Try to find a target if we don't have one
        if (target == null)
        {
            FindNearestTarget();
        }

        // Only active if we have a target
        if (target == null)
        {
            return 0;
        }

        // Check if unit is within party tether range
        if (networkRunService != null && networkRunService.Party != null)
        {
            Vector3 unitPosition = transform.position;
            Vector3 partyCenter = networkRunService.PartyCenterGround;
            float maxDistance = networkRunService.MaxTetherDistance;

            float distanceFromParty = Vector3.Distance(unitPosition, partyCenter);

            // Only combat if within tether range
            if (distanceFromParty <= maxDistance)
            {
                // Highest priority (higher than foraging which is 100)
                return 150;
            }
        }

        // Not in tether range, don't combat
        return 0;
    }

    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        if (agent != null)
        {
            agent.isStopped = true;
        }
        isAttacking = false;
        StopAllCoroutines();
    }

    public override void UnpauseBehaviour()
    {
        base.UnpauseBehaviour();
        StartCoroutine(CombatBehaviour());
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (target != null && target.Death != null)
        {
            target.Death.OnUnitDeath -= OnTargetDied;
        }
    }
}
