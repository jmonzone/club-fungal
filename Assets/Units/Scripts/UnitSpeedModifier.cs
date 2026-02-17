using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public struct NavMeshAreaSpeedModifier
{
    public int areaIndex;
    [Tooltip("Speed multiplier when on this area (e.g., 0.5 = half speed)")]
    public float speedMultiplier;
}

public class UnitSpeedModifier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private NavMeshAreaConfig navMeshAreaConfig;

    [Header("NavMesh Area Speed Modifiers")]
    [Tooltip("Area-specific speed multipliers. Leave empty to use default config values.")]
    [SerializeField] private NavMeshAreaSpeedModifier[] areaSpeedModifiers;

    [Header("Runtime")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private List<float> speedModifiers = new List<float>();
    [SerializeField] private float currentAreaSpeedMultiplier = 1f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (networkRunService)
        {
            baseSpeed = networkRunService.Settings.baseMovementSpeed;
        }
        else
        {
            baseSpeed = agent.speed;
        }
        UpdateSpeed();
    }

    private void Update()
    {
        CheckNavMeshArea();
    }

    private void CheckNavMeshArea()
    {
        if (!agent || !agent.isOnNavMesh) return;

        // Sample the NavMesh at the agent's current position
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            // Get the area index from the hit mask (bit index of the first set bit)
            int currentArea = GetAreaIndexFromMask(hit.mask);

            // Check if this area has a speed modifier
            float newAreaMultiplier = 1f;
            foreach (var areaModifier in areaSpeedModifiers)
            {
                if (areaModifier.areaIndex == currentArea)
                {
                    newAreaMultiplier = areaModifier.speedMultiplier;
                    break;
                }
            }

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

        agent.speed = baseSpeed * CurrentSpeedMultiplier;
    }
}
