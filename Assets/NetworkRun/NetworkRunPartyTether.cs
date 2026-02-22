using UnityEngine;

public class NetworkRunPartyTether : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private CameraService cameraService;
    [SerializeField] private NavMeshAreaConfig navMeshAreaConfig;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float teleportDistance = 20f;
    [SerializeField] private float spreadRadius = 2f;

    private Vector3 partyLeaderPosition;

    private void Awake()
    {
        CheckAndTetherUnits();
    }

    private void Update()
    {
        CheckAndTetherUnits();

        // Cycle party leader with P key
        if (Input.GetKeyDown(KeyCode.P))
        {
            networkRunService?.PartyService?.CyclePartyLeader();
        }
    }

    private void CheckAndTetherUnits()
    {
        if (networkRunService == null || networkRunService.PartyService == null || networkRunService.PartyService.Party == null)
            return;

        var partyLeader = networkRunService.PartyService.PartyLeader;
        if (partyLeader == null)
            return;

        // Use party leader's position as the center
        partyLeaderPosition = partyLeader.transform.position;
        networkRunService.SetPartyTetherData(partyLeaderPosition, maxDistance, Vector3.zero);

        // Give units return positions when camera is moving
        bool cameraIsMoving = cameraService != null && cameraService.IsMoving;

        // Check each party member's distance (skip the leader)
        int index = 0;
        foreach (var unitInstance in networkRunService.PartyService.Party.Units)
        {
            var controller = unitControllerService.Controllers.Find(c => c.Instance == unitInstance);
            if (controller == null || controller == partyLeader) continue; // Skip party leader

            Vector3 unitPosition = controller.transform.position;
            float currentDistance = Vector3.Distance(unitPosition, partyLeaderPosition);

            // Give return position if camera is moving
            if (cameraIsMoving)
            {
                // Calculate spread position around party leader
                float angle = index * 360f / (networkRunService.PartyService.Party.Units.Count - 1) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * spreadRadius, 0, Mathf.Sin(angle) * spreadRadius);
                Vector3 spreadPosition = partyLeaderPosition + offset;

                // Try to find a position on NavMesh - aqua units prefer water/slow terrain
                if (navMeshAreaConfig != null)
                {
                    bool isAquaUnit = unitInstance.Species?.Type?.Id == "aqua";
                    spreadPosition = navMeshAreaConfig.FindBestNavMeshPosition(spreadPosition, partyLeaderPosition, true, maxDistance, isAquaUnit);
                }

                // Set return position on all behaviors that support it
                var returnPositionables = controller.GetComponents<IReturnPositionable>();
                bool shouldTeleport = currentDistance > teleportDistance;
                foreach (var returnPositionable in returnPositionables)
                {
                    returnPositionable.SetReturnPosition(spreadPosition, shouldTeleport);
                }
            }

            index++;
        }
    }

    private void OnDrawGizmos()
    {
        if (partyLeaderPosition == Vector3.zero) return;

        // Draw max distance circle (yellow)
        Gizmos.color = Color.yellow;
        DrawCircle(partyLeaderPosition, maxDistance, 64);

        // Draw teleport distance circle (red)
        Gizmos.color = Color.red;
        DrawCircle(partyLeaderPosition, teleportDistance, 64);

        // Draw center point
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(partyLeaderPosition, 0.5f);
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
