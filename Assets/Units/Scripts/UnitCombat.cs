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
    private Vector3 returnPosition;

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
        if (unitControllerService == null || networkRunService == null || networkRunService.PartyService == null || networkRunService.PartyService.Party == null)
            return;

        UnitController nearestEnemy = null;

        // Detection range is party tether range + offset
        float detectionRange = networkRunService.MaxTetherDistance + detectionRangeOffset;
        float nearestDistance = detectionRange;

        foreach (var controller in unitControllerService.Controllers)
        {
            // Skip null or destroyed controllers
            if (controller == null)
                continue;

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
        returnPosition = position;

        // Only handle teleporting for combat units
        if (shouldTeleport)
        {
            Controller.Teleport(position, Controller.transform.parent);
        }
    }

    protected override void Update()
    {
        base.Update();

        // Auto-find target periodically
        if (Time.time - lastTargetSearchTime >= targetSearchInterval)
        {
            FindNearestTarget();
            lastTargetSearchTime = Time.time;
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
            // If no target, just wait
            if (target == null)
            {
                yield return null;
                continue;
            }

            float distance = Vector3.Distance(transform.position, target.transform.position);

            // Calculate optimal combat position (close to return position while in attack range)
            Vector3 combatPosition = CalculateOptimalCombatPosition();

            // Move to optimal position and face target
            Controller.SetLookPosition(target.transform.position);
            Controller.Destination.SetDestination(combatPosition);

            // Attack if in range and enough time has passed
            if (distance <= attackRange && Time.time - lastAttackTime >= attackInterval)
            {
                ThrowProjectile();
                lastAttackTime = Time.time;
            }

            yield return null;
        }
    }

    private Vector3 CalculateOptimalCombatPosition()
    {
        if (target == null || networkRunService == null || networkRunService.PartyService == null || networkRunService.PartyService.Party == null)
            return transform.position;

        Vector3 targetPosition = target.transform.position;
        float optimalDistance = attackRange * 0.9f;

        // If return position is set
        if (returnPosition != Vector3.zero)
        {
            float distanceToTarget = Vector3.Distance(returnPosition, targetPosition);

            // Return position is in attack range - use it
            if (distanceToTarget <= attackRange)
            {
                return returnPosition;
            }

            // Return position is out of range - find closest point to it that IS in range
            Vector3 directionToReturn = returnPosition - targetPosition;

            // Safety check: if target is at/very near return position, use current position
            if (directionToReturn.sqrMagnitude < 0.01f)
            {
                return transform.position;
            }

            directionToReturn.Normalize();
            return targetPosition + (directionToReturn * optimalDistance);
        }

        // No return position set - use party center as fallback
        Vector3 partyCenter = networkRunService.PartyCenterGround;
        Vector3 directionToParty = partyCenter - targetPosition;

        // Safety check: if target is at/very near party center, use current position
        if (directionToParty.sqrMagnitude < 0.01f)
        {
            return transform.position;
        }

        directionToParty.Normalize();
        return targetPosition + (directionToParty * optimalDistance);
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
