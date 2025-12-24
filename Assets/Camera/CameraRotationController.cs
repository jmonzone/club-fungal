using UnityEngine;
using Cinemachine;

public class CameraRotationController : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public CinemachineVirtualCamera vCam;
    public PlayerService playerReference;

    [Header("Components - Auto-configured")]
    [SerializeField][ReadOnly] CameraFollowComponent followComponent;
    [SerializeField][ReadOnly] CameraOrbitComponent orbitComponent;
    [SerializeField][ReadOnly] CameraZoomComponent zoomComponent;
    [SerializeField][ReadOnly] CameraInputHandler inputHandler;

    void Start()
    {
        if (vCam == null) Debug.LogError("CameraRotationController: vCam not assigned!");

        // Get or add components
        followComponent = GetComponent<CameraFollowComponent>();
        if (followComponent == null) followComponent = gameObject.AddComponent<CameraFollowComponent>();

        orbitComponent = GetComponent<CameraOrbitComponent>();
        if (orbitComponent == null) orbitComponent = gameObject.AddComponent<CameraOrbitComponent>();

        zoomComponent = GetComponent<CameraZoomComponent>();
        if (zoomComponent == null) zoomComponent = gameObject.AddComponent<CameraZoomComponent>();

        inputHandler = GetComponent<CameraInputHandler>();
        if (inputHandler == null) inputHandler = gameObject.AddComponent<CameraInputHandler>();

        // Setup component references
        inputHandler.zoomComponent = zoomComponent;
        zoomComponent.vCam = vCam;

        // Initialize orbit component with current rotation
        Vector3 e = transform.eulerAngles;
        float normalizedX = (e.x > 180f) ? (e.x - 360f) : e.x;
        orbitComponent.Initialize(e.y, normalizedX);

        // Initialize zoom component
        zoomComponent.Initialize();
    }

    void Update()
    {
        if (target == null || vCam == null) return;

        // Apply camera behavior
        followComponent.ApplyFollow(target);
        orbitComponent.ApplyRotation(zoomComponent.CurrentDistance, zoomComponent.minDistance, zoomComponent.maxDistance);
        zoomComponent.ApplyZoom();

        // Handle input
        inputHandler.HandleInput();
    }

    public void SetCanOrbit(bool canOrbit)
    {
        if (inputHandler != null) inputHandler.SetCanOrbit(canOrbit);
    }
}
