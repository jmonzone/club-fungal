using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitSpeedModifier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private UnitController unitController;

    [Header("Runtime")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private List<float> speedModifiers = new List<float>();
    [SerializeField] private float currentAreaSpeedMultiplier = 1f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        unitController = GetComponent<UnitController>();
    }

    private void Start()
    {
        UpdateSpeed();
    }

    private void Update()
    {
        CheckNavMeshArea();
    }

    private void CheckNavMeshArea()
    {
        if (!agent || !agent.isOnNavMesh || !unitController || unitController.Instance?.Species?.Type == null) return;

        // Sample the NavMesh at the agent's current position
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            // Get the area index from the hit mask
            int currentArea = GetAreaIndexFromMask(hit.mask);

            // Get speed multiplier from unit type's terrain modifiers
            float newAreaMultiplier = unitController.Instance.Species.Type.GetSpeedMultiplierForTerrain(currentArea);

            // Update speed if area multiplier changed
            if (Mathf.Abs(currentAreaSpeedMultiplier - newAreaMultiplier) > 0.001f)
            {
                currentAreaSpeedMultiplier = newAreaMultiplier;
                UpdateSpeed();
            }
        }
    }

    private int GetAreaIndexFromMask(int mask)
    {
        // Find the first set bit in the mask (the area index)
        for (int i = 0; i < 32; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                return i;
            }
        }
        return 0;
    }

    public void AddSpeedModifier(float modifier)
    {
        speedModifiers.Add(modifier);
        UpdateSpeed();
    }

    public void RemoveSpeedModifier(float modifier)
    {
        speedModifiers.Remove(modifier);
        UpdateSpeed();
    }

    public void ClearModifiers()
    {
        speedModifiers.Clear();
        UpdateSpeed();
    }

    public float CurrentSpeedMultiplier
    {
        get
        {
            float totalModifier = currentAreaSpeedMultiplier;
            foreach (var modifier in speedModifiers)
            {
                totalModifier *= modifier;
            }
            return totalModifier;
        }
    }

    private void UpdateSpeed()
    {
        if (!agent) return;

        baseSpeed = networkRunService.Settings.baseMovementSpeed;
        agent.speed = baseSpeed * CurrentSpeedMultiplier;
    }
}
