using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class UnitDestination : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private Vector3 destination;
    [SerializeField] private Transform target;
    [SerializeField] private bool isAtDestination = true;

    public Vector3 Destination => destination;
    public Transform Target => target;
    public bool IsAtDestination => isAtDestination;

    private NavMeshAgent navMeshAgent;

    public event UnityAction OnTargetSelected;

    private void OnValidate()
    {
        Initialize();
    }

    protected void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;

        // Enable avoidance to prevent units from stacking
        navMeshAgent.radius = 0.5f;
        navMeshAgent.avoidancePriority = Random.Range(0, 100);
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    public void SetDestination(Vector3 destination)
    {
        //Debug.Log("Setting destination");
        target = null;
        this.destination = destination;
        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        isAtDestination = false;
        navMeshAgent.SetDestination(destination);
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        isAtDestination = false;

        OnTargetSelected?.Invoke();
    }

    public void TeleportToPosition(Vector3 position)
    {
        target = null;
        navMeshAgent.enabled = false;
        navMeshAgent.Warp(position);
        transform.position = position;
        destination = position;
        isAtDestination = true;
        navMeshAgent.enabled = true;
    }

    private void Update()
    {
        if (target)
        {
            // Direction from self to target
            Vector3 direction = (target.position - transform.position).normalized;

            // Desired stopping position 1 unit away
            Vector3 offsetDestination = target.position - direction * 1f;

            destination = offsetDestination;
            navMeshAgent.SetDestination(destination);
        }

        if (!isAtDestination && Vector3.Distance(destination, transform.position) < 0.5f)
        {
            isAtDestination = true;
        }
    }
}
