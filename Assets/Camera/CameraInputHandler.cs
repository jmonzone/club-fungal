using UnityEngine;
using UnityEngine.EventSystems;

public class CameraInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraService cameraService;
    public CameraZoomComponent zoomComponent;
    public CameraPanComponent panComponent;
    [SerializeField] private VirtualJoystick virtualJoystick;

    [Header("Pan Settings")]
    public float panTouchSensitivity = 0.01f;
    public float panMouseSensitivity = 10f;
    public float joystickPanSensitivity = 0.01f;

    [Header("Read Only")]
    [SerializeField][ReadOnly] bool canOrbit = true;
    [SerializeField][ReadOnly] bool dragging;

    Vector2 lastMousePos;

    private void OnEnable()
    {
        if (virtualJoystick != null)
        {
            virtualJoystick.OnJoystickUpdate += HandleJoystickPan;
        }
    }

    private void OnDisable()
    {
        if (virtualJoystick != null)
        {
            virtualJoystick.OnJoystickUpdate -= HandleJoystickPan;
        }
    }

    private void HandleJoystickPan(Vector3 direction)
    {
        if (panComponent != null)
        {
            Vector2 delta = new Vector2(direction.x, direction.y);
            panComponent.AddPan(delta * joystickPanSensitivity, true);
        }
    }

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

        if (Application.isEditor)
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

        // Skip touch panning if virtual joystick is active
        if (virtualJoystick != null && virtualJoystick) return;

        // One finger swipe -> pan
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Moved)
            {
                panComponent.AddPan(-t.deltaPosition * panTouchSensitivity, true);
            }
        }
        else if (Application.isEditor && Input.GetMouseButton(0))
        {
            // Shift + Left mouse drag (Editor) -> pan
            Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            panComponent.AddPan(-delta * panMouseSensitivity, false);
        }
    }
}
