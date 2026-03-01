using UnityEngine;
using UnityEngine.EventSystems;

public class NetworkRunPartyTetherIndicator : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private GameObject indicatorObject;
    [SerializeField] private VirtualJoystick virtualJoystick;
    [SerializeField] private float selectionRadius = 1.5f;

    private float groundY;

    private void Start()
    {
        if (indicatorObject != null)
        {
            groundY = indicatorObject.transform.position.y;
        }
    }

    private void Update()
    {
        UpdateIndicatorPosition();
        // HandleClickInput();
    }

    private void UpdateIndicatorPosition()
    {
        if (networkRunService == null || indicatorObject == null) return;

        Vector3 partyCenter = networkRunService.PartyCenterGround;

        // Only update if we have a valid position
        if (partyCenter != Vector3.zero)
        {
            Vector3 targetPosition = partyCenter;

            // Check if party leader is diving (submerged)
            bool isSubmerged = false;
            var partyLeader = networkRunService.PartyService?.PartyLeader;
            if (partyLeader != null && partyLeader.Instance != null)
            {
                var submergedComponent = partyLeader.GetComponentInstance<SubmergedComponentInstance>();
                if (submergedComponent != null && submergedComponent.IsSubmerged)
                {
                    isSubmerged = true;
                }
            }

            // If submerged, follow the actual depth; otherwise stay at ground level
            if (isSubmerged)
            {
                targetPosition.y = partyCenter.y;
            }
            else
            {
                targetPosition.y = groundY;
            }

            indicatorObject.transform.position = targetPosition;
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

            // Skip if touch is over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) continue;

            // Check if this touch hits the indicator
            if (CheckRaycastHit(touch.position))
            {
                SelectPartyLeader(touch.position);
                return;
            }
        }

        // Mouse fallback for editor (only if virtual joystick isn't active with mouse)
        if (Input.GetMouseButtonDown(0))
        {
            // Skip if mouse is over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            if (virtualJoystick == null || !virtualJoystick.IsActive)
            {
                if (CheckRaycastHit(Input.mousePosition))
                {
                    SelectPartyLeader(Input.mousePosition);
                }
            }
        }
    }

    private void SelectPartyLeader(Vector3 screenPosition)
    {
        if (networkRunService?.PartyService == null) return;

        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        // Try to find a party member with a spherecast
        RaycastHit[] hits = Physics.SphereCastAll(ray, selectionRadius, 100f);

        UnitController selectedController = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            var controller = hit.transform.GetComponentInParent<UnitController>();
            if (controller != null && networkRunService.PartyService.PartyControllers.Contains(controller))
            {
                // Find the closest party member if multiple are in range
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    selectedController = controller;
                }
            }
        }

        // If we found a party member, select them; otherwise cycle
        if (selectedController != null)
        {
            networkRunService.PartyService.SetPartyLeader(selectedController);
        }
        else
        {
            networkRunService.PartyService.CyclePartyLeader();
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
