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
    [SerializeField] private float yOffset = 0f;

    public Vector3 Destination => destination;
    public Transform Target => target;
    public bool IsAtDestination => isAtDestination;
    public float YOffset => yOffset;

    private NavMeshAgent navMeshAgent;

    public event UnityAction OnTargetSelected;

    protected void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
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

    /// <summary>
    /// Set a Y-axis offset to apply to the unit's position.
    /// Useful for underwater positioning or floating effects.
    /// </summary>
    public void SetYOffset(float offset)
    {
        yOffset = offset;
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

        // Apply Y-offset after NavMeshAgent updates position
        if (Mathf.Abs(yOffset) > 0.001f)
        {
            Vector3 pos = transform.position;
            pos.y += yOffset;
            transform.position = pos;
        }
    }
}
