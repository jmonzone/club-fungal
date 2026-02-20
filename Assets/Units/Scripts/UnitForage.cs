using System.Collections;
using NUnit.Framework;
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
    private Vector3 targetPosition;
    private NavMeshAgent agent;
    private UnitSpeedModifier speedModifier;
    private bool isForaging = false;
    private PlantSporeEmitter currentPlant;

    public IForageTarget CurrentTarget => target;

    public event UnityAction OnForageTargetReached;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        speedModifier = GetComponent<UnitSpeedModifier>();
        agent.updateRotation = false;
    }

    private void OnDisable()
    {
        CleanupPlantEvents();
    }

    public void SetTarget(IForageTarget target, Vector3 position = default)
    {
        this.target = target;
        // If no position provided, use target's position
        this.targetPosition = position != default ? position : (target?.Transform.position ?? Vector3.zero);
        // Priority system will automatically activate/deactivate this behavior
    }

    protected override void OnBehaviourStart()
    {
        agent.isStopped = false;
        isForaging = false;
        StartCoroutine(ForagingBehaviour());
    }

    protected override void OnBehaviourStop()
    {
        agent.isStopped = true;

        // Cancel any ongoing forage
        if (currentPlant != null)
        {
            currentPlant.CancelForage();
        }

        CleanupPlantEvents();
        isForaging = false;
        StopAllCoroutines();
    }

    private IEnumerator ForagingBehaviour()
    {
        Debug.Log("Starting foraging behavior towards target: " + target.Transform.name);

        while (target != null)
        {
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (!isForaging && distance <= 2f)
            {
                // Stop moving and start foraging
                agent.isStopped = true;
                isForaging = true;

                // Get plant reference and subscribe to events
                currentPlant = (target as MonoBehaviour)?.GetComponent<PlantSporeEmitter>();
                if (currentPlant != null)
                {
                    currentPlant.OnMushroomForaged += HandleForageComplete;
                    currentPlant.OnForageCancelled += HandleForageCancelled;
                }

                // Get speed multiplier from UnitSpeedModifier
                float speedMultiplier = speedModifier != null ? speedModifier.CurrentSpeedMultiplier : 1f;

                // Trigger the forage (plant will start channeling with speed multiplier)
                if (currentPlant != null)
                {
                    currentPlant.StartForage(Controller, speedMultiplier);

                    Handheld.Vibrate();
                }
                else
                {
                    target.OnForaged(Controller);
                }

                Controller.SetLookPosition(target.Transform.position);
                OnForageTargetReached?.Invoke();
            }
            else if (isForaging)
            {
                // While foraging, stay stopped and face the target
                agent.isStopped = true;
                Controller.SetLookPosition(target.Transform.position);

                // Plant will handle cancellation if we move too far
            }
            else
            {
                // Moving towards assigned position around target
                Controller.Destination.SetDestination(targetPosition);
                Controller.SetLookPosition(target.Transform.position);
            }

            yield return null;
        }

        StopBehaviour();
    }

    private void HandleForageComplete()
    {
        CleanupPlantEvents();
        target = null;
        isForaging = false;
    }

    private void HandleForageCancelled()
    {
        CleanupPlantEvents();
        isForaging = false;
        // Keep target so we can try again
    }

    private void CleanupPlantEvents()
    {
        if (currentPlant != null)
        {
            currentPlant.OnMushroomForaged -= HandleForageComplete;
            currentPlant.OnForageCancelled -= HandleForageCancelled;
            currentPlant = null;
        }
    }

    public override void PauseBehaviour()
    {
        base.PauseBehaviour();
        agent.isStopped = true;

        // Cancel any ongoing forage when paused
        if (currentPlant != null)
        {
            currentPlant.CancelForage();
        }

        CleanupPlantEvents();
        isForaging = false;
        StopAllCoroutines();
    }

    public override void UnpauseBehaviour()
    {
        base.UnpauseBehaviour();
        StartCoroutine(ForagingBehaviour());
    }

}
