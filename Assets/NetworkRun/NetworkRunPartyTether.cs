using System.Collections.Generic;
using UnityEngine;

public class NetworkRunPartyTether : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float teleportDistance = 20f;
    [SerializeField] private float spreadRadius = 2f;

    private Camera mainCamera;
    private Vector3 partyCenterGround;
    private Vector3 previousCenterGround;
    private Vector3 tetherVelocity;
    [SerializeField] private float anticipationTime = 1.5f; // How far ahead to predict
    [SerializeField] private float minVelocityThreshold = 1f; // Min speed to trigger anticipation
    private HashSet<UnitInstance> unitsOutsideTether = new HashSet<UnitInstance>();

    private void Awake()
    {
        mainCamera = Camera.main;
        previousCenterGround = Vector3.zero;
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

        // Calculate tether velocity
        if (previousCenterGround != Vector3.zero)
        {
            tetherVelocity = (partyCenterGround - previousCenterGround) / Time.deltaTime;
        }
        previousCenterGround = partyCenterGround;

        networkRunService.SetPartyTetherData(partyCenterGround, maxDistance);

        // Check if tether is moving fast enough to anticipate
        float tetherSpeed = tetherVelocity.magnitude;
        bool shouldAnticipate = tetherSpeed > minVelocityThreshold;

        // Check each party member's distance
        int index = 0;
        foreach (var unitInstance in networkRunService.Party.Unit)
        {
            var controller = unitControllerService.Controllers.Find(c => c.Instance == unitInstance);
            if (controller == null) continue;

            Vector3 unitPosition = controller.transform.position;
            float currentDistance = Vector3.Distance(unitPosition, partyCenterGround);

            // Predict future distance if tether is moving
            float effectiveDistance = currentDistance;
            if (shouldAnticipate)
            {
                Vector3 predictedTetherPosition = partyCenterGround + (tetherVelocity * anticipationTime);
                float predictedDistance = Vector3.Distance(unitPosition, predictedTetherPosition);
                effectiveDistance = Mathf.Max(currentDistance, predictedDistance);
            }

            bool isOutside = effectiveDistance > maxDistance;
            bool wasOutside = unitsOutsideTether.Contains(unitInstance);

            // Track unit re-entering tether
            if (wasOutside && !isOutside)
            {
                unitsOutsideTether.Remove(unitInstance);
                networkRunService.NotifyUnitReenteredTether();
            }
            else if (!wasOutside && isOutside)
            {
                unitsOutsideTether.Add(unitInstance);
            }

            // Trigger return if currently too far OR predicted to be too far
            if (isOutside)
            {
                // Calculate spread position in a circle around ground center
                float angle = (index * 360f / networkRunService.Party.Unit.Count) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * spreadRadius, 0, Mathf.Sin(angle) * spreadRadius);
                Vector3 spreadPosition = partyCenterGround + offset;

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
