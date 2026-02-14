using UnityEngine;
using System.Linq;

public class NetworkRunBehaviour : UnitBehaviour
{
    [Header("Scan Settings")]
    [SerializeField] private float scanRadius = 10f;
    [SerializeField] private float forageDistance = 1.5f; // How close to get before foraging
    [SerializeField] private float forageCooldown = 2f; // Time between foraging attempts

    [Header("Activity Priorities (0-1)")]
    [SerializeField] private float inspectPriority = 0.7f;
    [SerializeField] private float collectPriority = 0.8f;
    [SerializeField] private float foragePriority = 0.9f;

    [Header("References")]
    [SerializeField] private NetworkRunService networkRunService;

    private IForageTarget currentTarget;
    private float nextForageTime;

    public override int GetPriority()
    {
        // Only active during network runs
        if (networkRunService == null || networkRunService.Party == null) return 0;
        return 50; // Higher than default wander, lower than manual control
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnBehaviourStart()
    {
    }

    public override void StopBehaviour()
    {
        base.StopBehaviour();
        currentTarget = null;
    }

    protected override void Update()
    {
        base.Update();

        if (!IsActive) return;

        // Check if we're within party tether range
        if (!IsInPartyTetherRange())
        {
            MoveBackToParty();
            return;
        }

        // If we have a target, move to it and try to forage
        if (currentTarget != null)
        {
            if (!currentTarget.IsAvailable)
            {
                // Target no longer available, clear it
                currentTarget = null;
                return;
            }

            // Check if we're in range to forage
            float distanceToTarget = Vector3.Distance(Controller.transform.position, currentTarget.Transform.position);
            if (distanceToTarget <= forageDistance)
            {
                // In range - forage it
                bool isMushroom = (currentTarget as MonoBehaviour)?.GetComponent<PlantSporeEmitter>() != null;

                currentTarget.OnForaged(Controller);
                currentTarget = null;

                // Only apply cooldown if it was a mushroom
                if (isMushroom)
                {
                    nextForageTime = Time.time + forageCooldown;
                }

                return;
            }
            else
            {
                // Keep moving toward target
                if (Controller.Destination != null)
                {
                    Controller.Destination.SetDestination(currentTarget.Transform.position);
                    Controller.SetLookPosition(currentTarget.Transform.position);
                }
                return;
            }
        }

        // Scan for nearby interactive objects
        FindAndInteractWithEnvironment();
    }

    private bool IsInPartyTetherRange()
    {
        if (networkRunService == null) return true;

        float distance = Vector3.Distance(
            Controller.transform.position,
            networkRunService.PartyCenterGround
        );

        return distance <= networkRunService.MaxTetherDistance;
    }

    private void MoveBackToParty()
    {
        if (networkRunService == null) return;

        // Clear current target since we need to return
        currentTarget = null;

        // Move toward a random position within the tether range
        if (Controller.Destination != null)
        {
            Vector3 randomOffset = Random.insideUnitSphere * networkRunService.MaxTetherDistance * 0.7f;
            randomOffset.y = 0f; // Keep on same height
            Vector3 destination = networkRunService.PartyCenterGround + randomOffset;

            Controller.Destination.SetDestination(destination);
            Controller.SetLookPosition(destination);
        }
    }

    private void FindAndInteractWithEnvironment()
    {
        var nearbyColliders = Physics.OverlapSphere(Controller.transform.position, scanRadius);

        IForageTarget bestTarget = null;
        float bestScore = 0f;

        foreach (var collider in nearbyColliders)
        {
            // Check for forageable objects (mushrooms, spores, etc.)
            var forageTarget = collider.GetComponentInParent<IForageTarget>();
            if (forageTarget != null && forageTarget.IsAvailable)
            {
                // Only consider targets within party tether range
                if (!IsTargetInPartyTetherRange(forageTarget.Transform.position))
                {
                    continue;
                }

                // Determine target type - check spore first since it might be child of mushroom
                bool isSpore = collider.GetComponentInParent<SporeController>() != null;
                bool isMushroom = !isSpore && collider.GetComponentInParent<PlantSporeEmitter>() != null;

                // Skip mushrooms if on cooldown
                if (isMushroom && Time.time < nextForageTime)
                {
                    continue;
                }

                // Set priority based on type
                float priority;
                if (isMushroom)
                {
                    priority = 0.95f; // Mushrooms highest priority
                }
                else if (isSpore)
                {
                    priority = collectPriority; // Spores use collect priority
                }
                else
                {
                    priority = foragePriority; // Other forageable items
                }

                float score = CalculateTargetScore(forageTarget, priority);
                
                if (score > bestScore)
                {
                    bestTarget = forageTarget;
                    bestScore = score;
                }
            }
        }

        if (bestTarget != null)
        {
            StartForaging(bestTarget);
        }
    }

    private bool IsTargetInPartyTetherRange(Vector3 targetPosition)
    {
        if (networkRunService == null) return true;

        float distance = Vector3.Distance(
            targetPosition,
            networkRunService.PartyCenterGround
        );

        return distance <= networkRunService.MaxTetherDistance;
    }

    private float CalculateTargetScore(IForageTarget target, float basePriority)
    {
        // Factor in distance (closer = better)
        float distance = Vector3.Distance(Controller.transform.position, target.Transform.position);
        float distanceMultiplier = Mathf.Max(0.1f, 1f - (distance / scanRadius));

        return basePriority * distanceMultiplier;
    }

    private void StartForaging(IForageTarget target)
    {
        if (target == null) return;

        currentTarget = target;

        // Start moving toward the target
        if (Controller.Destination != null)
        {
            Controller.Destination.SetDestination(target.Transform.position);
            Controller.SetLookPosition(target.Transform.position);
        }
    }
}
