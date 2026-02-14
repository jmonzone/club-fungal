using UnityEngine;
using System.Linq;

public class NetworkRunBehaviour : UnitBehaviour
{
    [Header("Scan Settings")]
    [SerializeField] private float scanRadius = 10f;
    [SerializeField] private float scanInterval = 1f;

    [Header("Activity Priorities (0-1)")]
    [SerializeField] private float inspectPriority = 0.7f;
    [SerializeField] private float collectPriority = 0.8f;
    [SerializeField] private float foragePriority = 0.9f;

    [Header("References")]
    [SerializeField] private NetworkRunService networkRunService;

    private float nextScanTime;
    private IForageTarget currentTarget;

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
        nextScanTime = Time.time;
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
        if (Time.time < nextScanTime) return;

        nextScanTime = Time.time + scanInterval;

        // Check if we're within party tether range
        if (!IsInPartyTetherRange())
        {
            MoveBackToParty();
            return;
        }

        // Don't interrupt if already doing something
        if (currentTarget != null) return;

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
            // Check for forageable objects (spores, mushrooms, etc.)
            var forageTarget = collider.GetComponent<IForageTarget>();
            if (forageTarget != null && forageTarget.IsAvailable)
            {
                float score = CalculateTargetScore(forageTarget, foragePriority);
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

    private float CalculateTargetScore(IForageTarget target, float basePriority)
    {
        // Factor in need
        float needMultiplier = CalculateNeedMultiplier();
        if (needMultiplier == 0f) return 0f;

        // Factor in distance (closer = better)
        float distance = Vector3.Distance(Controller.transform.position, target.Transform.position);
        float distanceMultiplier = Mathf.Max(0.1f, 1f - (distance / scanRadius));

        return basePriority * needMultiplier * distanceMultiplier;
    }

    private float CalculateNeedMultiplier()
    {
        // Can't collect if inventory full
        if (Instance.Inventory.IsFull) return 0f;

        // Energy-based priority
        if (Instance.Energy < 50f) return 0.5f;

        return 1f;
    }

    private void StartForaging(IForageTarget target)
    {
        if (target == null) return;

        // Use the existing UnitForage behaviour if available
        var forageBehaviour = Controller.GetComponent<UnitForage>();
        if (forageBehaviour != null)
        {
            forageBehaviour.SetTarget(target);
            currentTarget = target;
        }
    }

    public void OnForageComplete()
    {
        currentTarget = null;
    }
}
