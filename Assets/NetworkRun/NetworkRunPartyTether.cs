using UnityEngine;

public class NetworkRunPartyTether : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private CameraService cameraService;
    [SerializeField] private UnitBehaviourPriorityService behaviourPriorityService;
    [SerializeField] private NavMeshAreaConfig navMeshAreaConfig;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float teleportDistance = 20f;
    [SerializeField] private float spreadRadius = 2f;

    private Camera mainCamera;
    private Vector3 partyCenterGround;

    private void Awake()
    {
        mainCamera = Camera.main;
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
        if (mainCamera == null || networkRunService == null || networkRunService.PartyService == null || networkRunService.PartyService.Party == null)
            return;

        // Use party leader position as center
        var partyLeader = networkRunService.PartyService.PartyLeader;
        if (partyLeader == null)
            return;

        partyCenterGround = partyLeader.transform.position;

        networkRunService.SetPartyTetherData(partyCenterGround, maxDistance, Vector3.zero);

        // Give units return positions when camera is moving
        bool cameraIsMoving = cameraService != null && cameraService.IsMoving;

        // Get party leader's forward direction for relative positioning (use render root for visual facing)
        Vector3 leaderForward = partyLeader.RenderRoot != null ? partyLeader.RenderRoot.forward : partyLeader.transform.forward;
        float leaderYaw = Mathf.Atan2(leaderForward.x, leaderForward.z);

        // Check each party member's distance
        int index = 0;
        foreach (var unitInstance in networkRunService.PartyService.Party.Units)
        {
            var controller = unitControllerService.Controllers.Find(c => c.Instance == unitInstance);
            if (controller == null) continue;

            // Skip party leader - they don't need a return position
            if (controller == partyLeader)
            {
                index++;
                continue;
            }

            // Skip units that are assigned to work (e.g., cooking)
            if (behaviourPriorityService != null && behaviourPriorityService.IsAssignedToWork(controller))
            {
                index++;
                continue;
            }

            Vector3 unitPosition = controller.transform.position;
            float currentDistance = Vector3.Distance(unitPosition, partyCenterGround);

            // Give return position if camera is moving
            if (cameraIsMoving)
            {
                // Calculate spread position relative to leader's facing direction
                float angle = leaderYaw + (index * 360f / networkRunService.PartyService.Party.Units.Count * Mathf.Deg2Rad);
                Vector3 offset = new Vector3(Mathf.Sin(angle) * spreadRadius, 0, Mathf.Cos(angle) * spreadRadius);
                Vector3 spreadPosition = partyCenterGround + offset;

                // Try to find a position on NavMesh - aqua units prefer water/slow terrain
                if (navMeshAreaConfig != null)
                {
                    bool isAquaUnit = unitInstance.Species?.Type?.Id == "aqua";
                    spreadPosition = navMeshAreaConfig.FindBestNavMeshPosition(spreadPosition, partyCenterGround, true, maxDistance, isAquaUnit);
                }

                // Set return position on all behaviors that support it
                var returnPositionables = controller.GetComponents<IReturnPositionable>();
                bool shouldTeleport = currentDistance > teleportDistance;
                foreach (var returnPositionable in returnPositionables)
                {
                    // Debug.Log($"Setting return position for {controller.Instance.DisplayName} to {spreadPosition} (distance from center: {currentDistance:F1}, teleport: {shouldTeleport})");
                    returnPositionable.SetReturnPosition(spreadPosition, shouldTeleport);
                }
            }

            index++;
        }
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
