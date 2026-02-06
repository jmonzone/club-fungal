using UnityEngine;
using UnityEngine.EventSystems;

public class CameraInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraService cameraService;
    public CameraZoomComponent zoomComponent;
    public CameraPanComponent panComponent;

    [Header("Pan Settings")]
    public float panTouchSensitivity = 0.01f;
    public float panMouseSensitivity = 10f;

    [Header("Read Only")]
    [SerializeField][ReadOnly] bool canOrbit = true;
    [SerializeField][ReadOnly] bool dragging;

    Vector2 lastMousePos;

    public void SetCanOrbit(bool canOrbit)
    {
        this.canOrbit = canOrbit;
    }

    public void HandleInput()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;

        if (canOrbit) HandleOrbitInput();
        HandleZoomInput();
        HandlePanInput();
    }

    void HandleOrbitInput()
    {
        if (zoomComponent == null) return;

        bool isFirstPerson = cameraService.CameraMode == CameraMode.FIRST_PERSON;

        // One finger touch -> yaw (and pitch in first-person)
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
            {
                float directionMultiplier = zoomComponent.CurrentDistance < 0 ? -1 : 1;
                cameraService.AddYaw(t.deltaPosition.x * directionMultiplier);

                if (isFirstPerson)
                {
                    cameraService.AddPitch(-t.deltaPosition.y * directionMultiplier);
                }
            }
        }
        else if (Application.isEditor)
        {
            // Mouse drag (Editor) -> yaw (and pitch in first-person)
            if (Input.GetMouseButtonDown(0))
            {
                dragging = true;
                lastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                dragging = false;
            }

            if (dragging)
            {
                Vector2 current = Input.mousePosition;
                Vector2 delta = current - lastMousePos;
                lastMousePos = current;

                float directionMultiplier = zoomComponent.CurrentDistance < 0 ? -1 : 1;
                cameraService.AddYaw(delta.x * directionMultiplier);

                if (isFirstPerson)
                {
                    cameraService.AddPitch(-delta.y * directionMultiplier);
                }
            }
        }
    }

    void HandleZoomInput()
    {
        if (zoomComponent == null) return;

        // Mouse scroll / trackpad - use mouseScrollDelta for better compatibility
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.001f)
        {
            zoomComponent.Zoom(scroll * zoomComponent.zoomSpeed);
        }

        // Pinch zoom
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prevT0 = t0.position - t0.deltaPosition;
            Vector2 prevT1 = t1.position - t1.deltaPosition;

            float prevDist = (prevT0 - prevT1).magnitude;
            float currDist = (t0.position - t1.position).magnitude;

            float delta = currDist - prevDist;

            zoomComponent.Zoom(delta * zoomComponent.zoomSpeed * 0.1f * Time.deltaTime);
        }
    }

    void HandlePanInput()
    {
        if (panComponent == null) return;

        // Two finger swipe -> pan (average the delta movement)
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // Only pan if both touches are moving (not just pinching)
            if (t0.phase == TouchPhase.Moved && t1.phase == TouchPhase.Moved)
            {
                Vector2 avgDelta = (t0.deltaPosition + t1.deltaPosition) * 0.5f;
                panComponent.AddPan(-avgDelta * panTouchSensitivity);
            }
        }
        else if (Application.isEditor && Input.GetMouseButton(0))
        {
            // Shift + Left mouse drag (Editor) -> pan
            Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            panComponent.AddPan(-delta * panMouseSensitivity);
        }
    }
}
