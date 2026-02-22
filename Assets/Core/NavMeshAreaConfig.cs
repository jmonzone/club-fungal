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
    /// <param name="maxSearchDistance">Maximum distance from fallback center</param>
    /// <param name="preferSlowTerrain">If true, prefer slow terrain (water) over walkable areas (for aqua units)</param>
    /// <returns>Best valid NavMesh position</returns>
    public Vector3 FindBestNavMeshPosition(Vector3 targetPosition, Vector3 fallbackCenter = default, bool applyPadding = true, float maxSearchDistance = float.MaxValue, bool preferSlowTerrain = false)
    {
        float[] searchRadii = { 0.5f, 1f, 2f, 5f, 10f };
        const int maxSmallRadiusIndex = 2; // Radii beyond this index mean area is too small/sparse

        // Determine preferred and fallback area masks based on preferSlowTerrain
        int preferredAreaMask = preferSlowTerrain ? SlowTerrainAreaMask : WalkableAreaMask;
        int fallbackAreaMask = preferSlowTerrain ? WalkableAreaMask : SlowTerrainAreaMask;

        NavMeshHit? firstFallbackHit = null;

        for (int i = 0; i < searchRadii.Length; i++)
        {
            float radius = searchRadii[i];

            // Try preferred area first
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, radius, preferredAreaMask))
            {
                if (fallbackCenter != default && Vector3.Distance(hit.position, fallbackCenter) > maxSearchDistance)
                    continue;

                // If found at small radius, use it (area is substantial)
                if (i <= maxSmallRadiusIndex)
                {
                    if (applyPadding && edgePadding > 0f)
                    {
                        Vector3 paddedPosition = FindBestPaddedPosition(hit.position, preferredAreaMask);
                        if (paddedPosition != hit.position)
                        {
                            if (fallbackCenter == default || Vector3.Distance(paddedPosition, fallbackCenter) <= maxSearchDistance)
                                return paddedPosition;
                        }
                    }
                    return hit.position;
                }
                // If found at large radius but we have a fallback, use fallback (preferred area is too small)
                if (firstFallbackHit.HasValue)
                    return firstFallbackHit.Value.position;
            }

            // Track first valid fallback position
            if (!firstFallbackHit.HasValue && NavMesh.SamplePosition(targetPosition, out NavMeshHit fallbackHit, radius, fallbackAreaMask))
            {
                if (fallbackCenter == default || Vector3.Distance(fallbackHit.position, fallbackCenter) <= maxSearchDistance)
                    firstFallbackHit = fallbackHit;
            }
        }

        // Use fallback if we found one
        if (firstFallbackHit.HasValue)
            return firstFallbackHit.Value.position;

        // Last resort: use original position even if on fallback terrain, as long as it's somewhat close to a NavMesh
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
