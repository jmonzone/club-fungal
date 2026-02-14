using UnityEngine;

public class NetworkRunPartyTether : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float teleportDistance = 20f;
    [SerializeField] private float checkInterval = 0.5f;
    [SerializeField] private float spreadRadius = 2f;

    private Camera mainCamera;
    private float nextCheckTime;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + checkInterval;

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

        Vector3 groundCenter = hit.point;

        // Check each party member's distance
        int index = 0;
        foreach (var unitInstance in networkRunService.Party.Unit)
        {
            var controller = unitControllerService.Controllers.Find(c => c.Instance == unitInstance);
            if (controller == null) continue;

            float distance = Vector3.Distance(controller.transform.position, groundCenter);
            if (distance > maxDistance)
            {
                // Calculate spread position in a circle around ground center
                float angle = (index * 360f / networkRunService.Party.Unit.Count) * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * spreadRadius, 0, Mathf.Sin(angle) * spreadRadius);
                Vector3 spreadPosition = groundCenter + offset;

                if (distance > teleportDistance)
                {
                    // Too far - teleport immediately
                    controller.Teleport(spreadPosition, controller.transform.parent);
                }
                else
                {
                    // Within range - make them walk back using return behavior
                    var returnBehaviour = controller.GetComponent<UnitReturnToParty>();
                    if (returnBehaviour != null)
                    {
                        returnBehaviour.SetReturnPosition(spreadPosition);
                    }
                }
            }

            index++;
        }
    }
}
