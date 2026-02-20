using UnityEngine;
using Cinemachine;

public class CameraRotationController : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public CinemachineVirtualCamera vCam;
    public PlayerService playerReference;

    [Header("Component Toggles")]
    public bool enableFollow = true;
    public bool enableOrbit = true;
    public bool enableZoom = true;
    public bool enablePan = true;
    public bool enableInputHandler = true;

    [Header("Components - Auto-configured")]
    [SerializeField][ReadOnly] CameraFollowComponent followComponent;
    [SerializeField][ReadOnly] CameraOrbitComponent orbitComponent;
    [SerializeField][ReadOnly] CameraZoomComponent zoomComponent;
    [SerializeField][ReadOnly] CameraPanComponent panComponent;
    [SerializeField][ReadOnly] CameraInputHandler inputHandler;

    [Header("Services")]
    [SerializeField] private CameraService cameraService;

    private Vector3 settledPosition;
    private bool hasStoredSettledPosition;

    void Start()
    {
        if (vCam == null) Debug.LogError("CameraRotationController: vCam not assigned!");

        // Get or add components based on toggles
        if (enableFollow)
        {
            followComponent = GetComponent<CameraFollowComponent>();
            if (followComponent == null) followComponent = gameObject.AddComponent<CameraFollowComponent>();
        }

        if (enableOrbit)
        {
            orbitComponent = GetComponent<CameraOrbitComponent>();
            if (orbitComponent == null) orbitComponent = gameObject.AddComponent<CameraOrbitComponent>();
        }

        if (enableZoom)
        {
            zoomComponent = GetComponent<CameraZoomComponent>();
            if (zoomComponent == null) zoomComponent = gameObject.AddComponent<CameraZoomComponent>();
        }

        if (enablePan)
        {
            panComponent = GetComponent<CameraPanComponent>();
            if (panComponent == null) panComponent = gameObject.AddComponent<CameraPanComponent>();
        }

        if (enableInputHandler)
        {
            inputHandler = GetComponent<CameraInputHandler>();
            if (inputHandler == null) inputHandler = gameObject.AddComponent<CameraInputHandler>();
        }

        // Setup component references
        if (inputHandler && zoomComponent) inputHandler.zoomComponent = zoomComponent;
        if (inputHandler && panComponent) inputHandler.panComponent = panComponent;
        if (zoomComponent) zoomComponent.vCam = vCam;

        // Initialize orbit component with current rotation
        if (orbitComponent)
        {
            Vector3 e = transform.eulerAngles;
            float normalizedX = (e.x > 180f) ? (e.x - 360f) : e.x;
            orbitComponent.Initialize(e.y, normalizedX);
        }

        // Initialize zoom component
        if (zoomComponent) zoomComponent.Initialize();

        // Initialize pan component
        if (panComponent) panComponent.Initialize();
    }

    void Update()
    {
        if (target == null || vCam == null) return;

        // Apply camera behavior
        if (followComponent) followComponent.ApplyFollow(target);
        if (panComponent) panComponent.ApplyPan();
        if (orbitComponent && zoomComponent) orbitComponent.ApplyRotation(zoomComponent.CurrentDistance, zoomComponent.minDistance, zoomComponent.maxDistance);
        if (zoomComponent) zoomComponent.ApplyZoom();

        // Handle input
        if (inputHandler) inputHandler.HandleInput();

        // Report movement to camera service
        if (cameraService)
        {
            float velocity = 0f;
            if (panComponent)
            {
                velocity = panComponent.driftVelocity.magnitude;
            }

            // Track displacement from settled position
            if (cameraService.HasSettled && !hasStoredSettledPosition)
            {
                settledPosition = transform.position;
                hasStoredSettledPosition = true;
            }
            else if (!cameraService.HasSettled)
            {
                hasStoredSettledPosition = false;
            }

            float displacement = hasStoredSettledPosition ? Vector3.Distance(transform.position, settledPosition) : 0f;

            cameraService.ReportMovement(velocity, displacement);
            cameraService.UpdateSettleTimer(Time.deltaTime);
        }
    }

    public void SetCanOrbit(bool canOrbit)
    {
        if (inputHandler != null) inputHandler.SetCanOrbit(canOrbit);
    }
}
