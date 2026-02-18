using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Config/NavMesh Area Config")]
public class NavMeshAreaConfig : ScriptableObject
{
    [Header("NavMesh Area Indices")]
    [Tooltip("Default walkable area (typically area 0)")]
    public int walkableArea = 0;

    [Tooltip("Slow terrain area that reduces movement speed")]
    public int slowTerrainArea = 6;

    [Tooltip("Other custom areas")]
    public int jumpArea = 2;
    public int notWalkableArea = 1;

    [Header("Spawn Padding")]
    [Tooltip("Distance to pad positions away from edges toward center")]
    public float edgePadding = 0.3f;

    /// <summary>
    /// Get the NavMesh area mask for the walkable area (1 << walkableArea)
    /// </summary>
    public int WalkableAreaMask => 1 << walkableArea;

    /// <summary>
    /// Get the NavMesh area mask for the slow terrain area (1 << slowTerrainArea)
    /// </summary>
    public int SlowTerrainAreaMask => 1 << slowTerrainArea;

    /// <summary>
    /// Get the NavMesh area mask for a specific area index
    /// </summary>
    public int GetAreaMask(int areaIndex) => 1 << areaIndex;

    /// <summary>
    /// Check if a NavMeshHit is on the walkable area
    /// </summary>
    public bool IsWalkableArea(UnityEngine.AI.NavMeshHit hit)
    {
        return (hit.mask & WalkableAreaMask) != 0;
    }

    /// <summary>
    /// Check if a NavMeshHit is on the slow terrain area
    /// </summary>
    public bool IsSlowTerrainArea(NavMeshHit hit)
    {
        return (hit.mask & SlowTerrainAreaMask) != 0;
    }

    /// <summary>
    /// Find the best NavMesh position on walkable area, with padding away from edges
    /// </summary>
    /// <param name="targetPosition">Desired position</param>
    /// <param name="fallbackCenter">Center point to pad toward (optional)</param>
    /// <param name="applyPadding">Whether to apply edge padding</param>
    /// <returns>Best valid NavMesh position</returns>
    public Vector3 FindBestNavMeshPosition(Vector3 targetPosition, Vector3 fallbackCenter = default, bool applyPadding = true)
    {
        // Try increasing search radii to find closest walkable position
        float[] searchRadii = { 0.5f, 1f, 2f, 5f, 10f };

        foreach (float radius in searchRadii)
        {
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, radius, WalkableAreaMask))
            {
                // Apply padding away from slow terrain edges
                if (applyPadding && edgePadding > 0f)
                {
                    Vector3 paddedPosition = FindBestPaddedPosition(hit.position, WalkableAreaMask);
                    if (paddedPosition != hit.position)
                    {
                        return paddedPosition;
                    }
                }

                return hit.position;
            }
        }

        // If no walkable area found, try slow terrain and pad toward walkable areas
        foreach (float radius in searchRadii)
        {
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit slowHit, radius, SlowTerrainAreaMask))
            {
                // Try to pad toward walkable area
                if (applyPadding && edgePadding > 0f)
                {
                    Vector3 paddedPosition = FindBestPaddedPosition(slowHit.position, WalkableAreaMask);
                    // If we found a walkable position nearby, use it
                    if (NavMesh.SamplePosition(paddedPosition, out NavMeshHit walkableCheck, 0.5f, WalkableAreaMask))
                    {
                        return walkableCheck.position;
                    }
                }

                return slowHit.position;
            }
        }

        // Fall back to any NavMesh position as last resort
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit anyHit, 10f, NavMesh.AllAreas))
        {
            return anyHit.position;
        }

        // Last resort: return the original target
        return targetPosition;
    }

    /// <summary>
    /// Find the best padded position by sampling directions to move deeper into target area
    /// </summary>
    private Vector3 FindBestPaddedPosition(Vector3 position, int targetAreaMask)
    {
        // Sample 8 directions to find which one has the most continuous target area
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized
        };

        int bestScore = -1;
        Vector3 bestDirection = Vector3.zero;

        foreach (Vector3 dir in directions)
        {
            int score = 0;
            // Check multiple distances in this direction
            for (float dist = edgePadding; dist <= edgePadding * 3f; dist += edgePadding)
            {
                Vector3 testPos = position + dir * dist;
                if (NavMesh.SamplePosition(testPos, out NavMeshHit hit, 0.5f, targetAreaMask))
                {
                    score++;
                }
                else
                {
                    break; // Stop checking this direction if we hit non-target area
                }
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = dir;
            }
        }

        // If we found a good direction, pad in that direction
        if (bestScore > 0)
        {
            Vector3 paddedPosition = position + bestDirection * edgePadding;
            if (NavMesh.SamplePosition(paddedPosition, out NavMeshHit paddedHit, 0.5f, targetAreaMask))
            {
                return paddedHit.position;
            }
        }

        return position;
    }

    /// <summary>
    /// Check if a position is on a walkable NavMesh area
    /// </summary>
    public bool IsPositionWalkable(Vector3 position, float sampleDistance = 1f)
    {
        return NavMesh.SamplePosition(position, out NavMeshHit hit, sampleDistance, WalkableAreaMask);
    }
}
