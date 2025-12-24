using UnityEngine;
using UnityEngine.EventSystems;

public class CameraInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraService cameraService;
    public CameraZoomComponent zoomComponent;

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
    }

    void HandleOrbitInput()
    {
        if (zoomComponent == null) return;

        // One finger touch -> yaw
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
            {
                float directionMultiplier = zoomComponent.CurrentDistance < 0 ? -1 : 1;
                cameraService.AddYaw(t.deltaPosition.x * directionMultiplier);
            }
        }
        else if (Application.isEditor)
        {
            // Mouse drag (Editor) -> yaw
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
}
