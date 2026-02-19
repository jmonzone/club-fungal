using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public abstract class NavMeshSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    [SerializeField] protected Collider spawnCollider;
    [SerializeField] protected NavMeshAreaConfig navMeshAreaConfig;

    [Header("Spawn Settings")]
    [SerializeField] protected int spawnCount = 10;
    [SerializeField] protected float minSpacing = 1f;
    [SerializeField] protected bool spawnOnAwake = true;

    protected List<Vector3> availableSpawnPositions = new List<Vector3>();
    protected List<Vector3> occupiedPositions = new List<Vector3>();

    protected virtual void Awake()
    {
        if (spawnOnAwake)
        {
            Spawn();
        }
    }

    public abstract void Spawn();

    protected List<Vector3> GenerateSpawnPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();

        if (spawnCollider == null)
        {
            Debug.LogWarning($"{GetType().Name}: No spawn collider assigned");
            return positions;
        }

        Bounds bounds = spawnCollider.bounds;
        int maxAttempts = count * 10;
        int attempts = 0;

        while (positions.Count < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Raycast down to find ground
            Vector3 finalPosition = randomPoint;
            if (Physics.Raycast(randomPoint + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                finalPosition = hit.point;
            }
            else
            {
                continue;
            }

            // Check if position is on valid NavMesh area (walkable)
            if (navMeshAreaConfig != null)
            {
                Vector3 boundsCenter = new Vector3(bounds.center.x, finalPosition.y, bounds.center.z);
                finalPosition = navMeshAreaConfig.FindBestNavMeshPosition(finalPosition, boundsCenter, true);

                if (!navMeshAreaConfig.IsPositionWalkable(finalPosition, 0.5f))
                {
                    continue;
                }
            }
            else
            {
                // Fallback without config
                int areaMask = 1 << 0;
                if (NavMesh.SamplePosition(finalPosition, out NavMeshHit navHit, 1f, areaMask))
                {
                    finalPosition = navHit.position;
                }
                else
                {
                    continue;
                }
            }

            // Check spacing from other positions
            if (IsValidSpacing(finalPosition, positions))
            {
                positions.Add(finalPosition);
            }
        }

        return positions;
    }

    protected bool IsValidSpacing(Vector3 position, List<Vector3> existingPositions)
    {
        foreach (Vector3 existing in existingPositions)
        {
            float distance = Vector3.Distance(position, existing);
            if (distance < minSpacing)
            {
                return false;
            }
        }
        return true;
    }

    protected Vector3 GetRandomAvailablePosition()
    {
        if (availableSpawnPositions.Count == 0)
        {
            Debug.LogWarning($"{GetType().Name}: No available spawn positions");
            return Vector3.zero;
        }

        int randomIndex = Random.Range(0, availableSpawnPositions.Count);
        Vector3 position = availableSpawnPositions[randomIndex];
        availableSpawnPositions.RemoveAt(randomIndex);
        occupiedPositions.Add(position);

        return position;
    }

    protected void FreePosition(Vector3 position)
    {
        if (occupiedPositions.Remove(position))
        {
            availableSpawnPositions.Add(position);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (spawnCollider != null)
        {
            Gizmos.color = GetGizmoColor();
            Gizmos.DrawCube(spawnCollider.bounds.center, spawnCollider.bounds.size);
        }
    }

    protected abstract Color GetGizmoColor();
}
