using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NetworkRunPartyTether : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private CameraService cameraService;
    [SerializeField] private NavMeshAreaConfig navMeshAreaConfig;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float teleportDistance = 20f;
    [SerializeField] private float spreadRadius = 2f;

    private Camera mainCamera;
    private Vector3 partyCenterGround;
    private HashSet<UnitInstance> unitsOutsideTether = new HashSet<UnitInstance>();

    private void Awake()
    {
        mainCamera = Camera.main;
        CheckAndTetherUnits();
    }

    private void Update()
    {
        CheckAndTetherUnits();
    }

    private void CheckAndTetherUnits()
    {
        if (mainCamera == null || networkRunService == null || networkRunService.Party == null)
            return;

        // Raycast from center of screen to ground
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
            return;

        partyCenterGround = hit.point;

        networkRunService.SetPartyTetherData(partyCenterGround, maxDistance, Vector3.zero);

        // Give units return positions when camera is moving
        bool cameraIsMoving = cameraService != null && cameraService.IsMoving;

        // Check each party member's distance
        int index = 0;
        foreach (var unitInstance in networkRunService.Party.Unit)
        {
            var controller = unitControllerService.Controllers.Find(c => c.Instance == unitInstance);
            if (controller == null) continue;

            Vector3 unitPosition = controller.transform.position;
            float currentDistance = Vector3.Distance(unitPosition, partyCenterGround);

            // Give return position if camera is moving
            if (cameraIsMoving)
            {
                // Calculate spread position
                float angle = (index * 360f / networkRunService.Party.Unit.Count) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * spreadRadius, 0, Mathf.Sin(angle) * spreadRadius);
                Vector3 spreadPosition = partyCenterGround + offset;

                // Try to find a position on NavMesh area 0 (Walkable) instead of slow terrain
                if (navMeshAreaConfig != null)
                {
                    spreadPosition = navMeshAreaConfig.FindBestNavMeshPosition(spreadPosition, partyCenterGround, true, maxDistance);
                }
                else
                {
                    spreadPosition = FindBestNavMeshPosition(spreadPosition, partyCenterGround, maxDistance);
                }

                // Use return behavior for both walking and teleporting
                var returnBehaviour = controller.GetComponent<UnitReturnToParty>();
                if (returnBehaviour != null)
                {
                    bool shouldTeleport = currentDistance > teleportDistance;
                    returnBehaviour.SetReturnPosition(spreadPosition, shouldTeleport);
                }
            }

            index++;
        }
    }

    // Fallback method if navMeshAreaConfig is not assigned
    private Vector3 FindBestNavMeshPosition(Vector3 targetPosition, Vector3 fallbackCenter, float maxSearchDistance)
    {
        int walkableAreaMask = 1 << 0;
        int slowTerrainAreaMask = 1 << 6;
        float edgePadding = 0.3f;
        float[] searchRadii = { 0.5f, 1f, 2f, 5f, 10f };

        // Try walkable areas first with directional padding (within maxSearchDistance)
        foreach (float radius in searchRadii)
        {
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, radius, walkableAreaMask))
            {
                // Check if position is within max distance from party center
                if (Vector3.Distance(hit.position, fallbackCenter) <= maxSearchDistance)
                {
                    Vector3 paddedPosition = FindBestPaddedPosition(hit.position, walkableAreaMask, edgePadding);
                    if (paddedPosition != hit.position && Vector3.Distance(paddedPosition, fallbackCenter) <= maxSearchDistance)
                    {
                        return paddedPosition;
                    }
                    return hit.position;
                }
            }
        }

        // If no walkable area found within range, try slow terrain (within maxSearchDistance)
        foreach (float radius in searchRadii)
        {
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit slowHit, radius, slowTerrainAreaMask))
            {
                // Check if position is within max distance from party center
                if (Vector3.Distance(slowHit.position, fallbackCenter) <= maxSearchDistance)
                {
                    // Try to pad toward walkable area
                    Vector3 paddedPosition = FindBestPaddedPosition(slowHit.position, walkableAreaMask, edgePadding);
                    // If we found a walkable position nearby and it's in range, use it
                    if (NavMesh.SamplePosition(paddedPosition, out NavMeshHit walkableCheck, 0.5f, walkableAreaMask))
                    {
                        if (Vector3.Distance(walkableCheck.position, fallbackCenter) <= maxSearchDistance)
                        {
                            return walkableCheck.position;
                        }
                    }
                    // Otherwise use the slow terrain position since it's in range
                    return slowHit.position;
                }
            }
        }

        // Last resort: use original position even if on slow terrain, as long as it's somewhat close to a NavMesh
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit anyHit, 2f, NavMesh.AllAreas))
        {
            return anyHit.position;
        }

        return targetPosition;
    }

    // Find the best padded position by sampling directions to move deeper into target area
    private Vector3 FindBestPaddedPosition(Vector3 position, int targetAreaMask, float edgePadding)
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

    private void OnDrawGizmos()
    {
        if (partyCenterGround == Vector3.zero) return;

        // Draw max distance circle (yellow)
        Gizmos.color = Color.yellow;
        DrawCircle(partyCenterGround, maxDistance, 64);

        // Draw teleport distance circle (red)
        Gizmos.color = Color.red;
        DrawCircle(partyCenterGround, teleportDistance, 64);

        // Draw center point
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(partyCenterGround, 0.5f);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
