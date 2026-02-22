using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Config/NavMesh Area Config")]
public class NavMeshAreaConfig : ScriptableObject
{
    [Header("NavMesh Area Indices")]
    [NavMeshAreaMask]
    [Tooltip("Walkable areas - select multiple areas that are considered walkable")]
    public int walkableAreaMask = 1; // Default: area 0 (Walkable)

    [NavMeshArea]
    [Tooltip("Slow terrain area that reduces movement speed")]
    public int slowTerrainArea = 6;

    [Header("Spawn Padding")]
    [Tooltip("Distance to pad positions away from edges toward center")]
    public float edgePadding = 0.3f;

    /// <summary>
    /// Get the NavMesh area mask for all walkable areas (combined from selection)
    /// </summary>
    public int WalkableAreaMask => walkableAreaMask;

    /// <summary>
    /// Get the NavMesh area mask for the slow terrain area (1 << slowTerrainArea)
    /// </summary>
    public int SlowTerrainAreaMask => 1 << slowTerrainArea;

    /// <summary>
    /// Find the best NavMesh position on walkable area, with padding away from edges
    /// </summary>
    /// <param name="targetPosition">Desired position</param>
    /// <param name="fallbackCenter">Center point to pad toward (optional)</param>
    /// <param name="applyPadding">Whether to apply edge padding</param>
    /// <returns>Best valid NavMesh position</returns>
    public Vector3 FindBestNavMeshPosition(Vector3 targetPosition, Vector3 fallbackCenter = default, bool applyPadding = true, float maxSearchDistance = float.MaxValue)
    {
        // Try increasing search radii to find closest walkable position
        float[] searchRadii = { 0.5f, 1f, 2f, 5f, 10f };

        foreach (float radius in searchRadii)
        {
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, radius, WalkableAreaMask))
            {
                // Check if position is within max distance from fallback center (if provided)
                if (fallbackCenter != default && Vector3.Distance(hit.position, fallbackCenter) > maxSearchDistance)
                {
                    continue; // Skip positions outside the search range
                }

                // Apply padding away from slow terrain edges
                if (applyPadding && edgePadding > 0f)
                {
                    Vector3 paddedPosition = FindBestPaddedPosition(hit.position, WalkableAreaMask);
                    // Validate padded position is also within range
                    if (paddedPosition != hit.position)
                    {
                        if (fallbackCenter == default || Vector3.Distance(paddedPosition, fallbackCenter) <= maxSearchDistance)
                        {
                            return paddedPosition;
                        }
                    }
                }

                return hit.position;
            }
        }

        // If no walkable area found within range, try slow terrain (within maxSearchDistance)
        foreach (float radius in searchRadii)
        {
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit slowHit, radius, SlowTerrainAreaMask))
            {
                // Check if position is within max distance from fallback center (if provided)
                if (fallbackCenter != default && Vector3.Distance(slowHit.position, fallbackCenter) > maxSearchDistance)
                {
                    continue; // Skip positions outside the search range
                }

                // Try to pad toward walkable area
                if (applyPadding && edgePadding > 0f)
                {
                    Vector3 paddedPosition = FindBestPaddedPosition(slowHit.position, WalkableAreaMask);
                    // If we found a walkable position nearby and it's in range, use it
                    if (NavMesh.SamplePosition(paddedPosition, out NavMeshHit walkableCheck, 0.5f, WalkableAreaMask))
                    {
                        if (fallbackCenter == default || Vector3.Distance(walkableCheck.position, fallbackCenter) <= maxSearchDistance)
                        {
                            return walkableCheck.position;
                        }
                    }
                }

                // Return slow terrain position since it's in range
                return slowHit.position;
            }
        }

        // Last resort: use original position even if on slow terrain, as long as it's somewhat close to a NavMesh
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit anyHit, 2f, NavMesh.AllAreas))
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
