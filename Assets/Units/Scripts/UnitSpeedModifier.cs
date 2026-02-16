using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitSpeedModifier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private NavMeshAgent agent;

    [Header("Runtime")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private List<float> speedModifiers = new List<float>();

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

    private void UpdateSpeed()
    {
        if (!agent) return;

        float totalModifier = 1f;
        foreach (var modifier in speedModifiers)
        {
            totalModifier *= modifier;
        }

        agent.speed = baseSpeed * totalModifier;
    }
}
