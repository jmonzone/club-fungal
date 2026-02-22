using UnityEngine;

public class NetworkRunPartyTetherIndicator : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private GameObject indicatorObject;
    [SerializeField] private VirtualJoystick virtualJoystick;

    private void Update()
    {
        UpdateIndicatorPosition();
        HandleClickInput();
    }

    private void UpdateIndicatorPosition()
    {
        if (networkRunService == null || indicatorObject == null) return;

        Vector3 partyCenter = networkRunService.PartyCenterGround;

        // Only update if we have a valid position
        if (partyCenter != Vector3.zero)
        {
            indicatorObject.transform.position = partyCenter;
        }
    }

    private void HandleClickInput()
    {
        if (networkRunService == null || indicatorObject == null) return;

        // Check for touch input (supports multiple touches)
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Only check touches that just began
            if (touch.phase != TouchPhase.Began) continue;

            // Skip touch if it's being used by the virtual joystick
            if (virtualJoystick != null && virtualJoystick.IsActive && IsJoystickTouch(i)) continue;

            // Check if this touch hits the indicator
            if (CheckRaycastHit(touch.position))
            {
                networkRunService?.PartyService?.CyclePartyLeader();
                return;
            }
        }

        // Mouse fallback for editor (only if virtual joystick isn't active with mouse)
        if (Input.GetMouseButtonDown(0))
        {
            if (virtualJoystick == null || !virtualJoystick.IsActive)
            {
                if (CheckRaycastHit(Input.mousePosition))
                {
                    networkRunService?.PartyService?.CyclePartyLeader();
                }
            }
        }
    }

    private bool IsJoystickTouch(int touchIndex)
    {
        // Check if this touch started on the joystick rect
        if (virtualJoystick == null || virtualJoystick.Rect == null) return false;

        Touch touch = Input.GetTouch(touchIndex);
        Vector2 localPoint;
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            virtualJoystick.Rect,
            touch.position,
            null,
            out localPoint
        ) && virtualJoystick.Rect.rect.Contains(localPoint);
    }

    private bool CheckRaycastHit(Vector3 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.transform == indicatorObject.transform || hit.transform.IsChildOf(indicatorObject.transform);
        }
        return false;
    }
}
