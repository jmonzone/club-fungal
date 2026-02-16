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
    private NavMeshAgent agent;
    private UnitSpeedModifier speedModifier;
    private bool isForaging = false;
    private PlantSporeEmitter currentPlant;

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

    public void SetTarget(IForageTarget target)
    {
        var previousTarget = this.target;
        this.target = target;
        if (previousTarget == null && target != null) InvokeOnBehaviourRequest();
        else if (target == null && !IsActive) StopBehaviour();
    }

    protected override void OnBehaviourStart()
    {
        agent.isStopped = false;
        isForaging = false;
        StartCoroutine(ForagingBehaviour());
    }

    private IEnumerator ForagingBehaviour()
    {
        Debug.Log("Starting foraging behavior towards target: " + target.Transform.name);

        while (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.Transform.position);

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
                // Moving towards target
                Controller.Destination.SetDestination(target.Transform.position);
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

    public override void StopBehaviour()
    {
        base.StopBehaviour();
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

    public override int GetPriority()
    {
        // Debug.Log("Checking forage behavior priority. Target: " + (target != null ? target.Transform.name : "None"));
        // High priority if we have a target
        return target != null ? 100 : 0;
    }

}
