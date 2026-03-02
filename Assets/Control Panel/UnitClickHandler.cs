using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles clicking on units in the world and selects them via ControlModeService.
/// When a unit is selected, the ControlPanelUI will show the unit detail page.
/// </summary>
public class UnitClickHandler : MonoBehaviour
{
    [SerializeField] private ControlModeService controlModeService;
    [SerializeField] private NetworkRunPartyService partyService;
    [SerializeField] private VirtualJoystick virtualJoystick;
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private float selectionRadius = 1.5f;
    [SerializeField] private bool allowSelectionInManualControl = false;

    private void Update()
    {
        // Only handle input when not in manual control mode (unless explicitly allowed)
        if (controlModeService != null && controlModeService.IsManualControlActive() && !allowSelectionInManualControl)
        {
            return;
        }

        HandleClickInput();
    }

    private void HandleClickInput()
    {
        // Check for touch input (supports multiple touches)
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Only check touches that just began
            if (touch.phase != TouchPhase.Began) continue;

            // Skip touch if it's being used by the virtual joystick
            if (virtualJoystick != null && virtualJoystick.IsActive && IsJoystickTouch(i)) continue;

            // Skip if touch is over UI (but allow clicking in empty space)
            if (IsPointerOverInteractableUI(touch.fingerId)) continue;

            // Try to select a unit at this touch position
            TrySelectUnit(touch.position);
            return;
        }

        // Mouse fallback for editor
        if (Input.GetMouseButtonDown(0))
        {
            // Skip if mouse is over UI (but allow clicking in empty space)
            if (IsPointerOverInteractableUI(-1)) return;

            // Skip if virtual joystick is active with mouse
            if (virtualJoystick != null && virtualJoystick.IsActive) return;

            TrySelectUnit(Input.mousePosition);
        }
    }

    private bool IsPointerOverInteractableUI(int touchId)
    {
        if (EventSystem.current == null) return false;

        // Check if we're over any UI element
        bool isOverUI = touchId < 0
            ? EventSystem.current.IsPointerOverGameObject()
            : EventSystem.current.IsPointerOverGameObject(touchId);

        if (!isOverUI) return false;

        // Get what we're actually over
        var pointerData = new UnityEngine.EventSystems.PointerEventData(EventSystem.current)
        {
            position = touchId < 0 ? (Vector2)Input.mousePosition : Input.GetTouch(touchId).position
        };

        var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // Check if any of the hit objects are actual interactable UI (buttons, toggles, etc)
        foreach (var result in results)
        {
            var selectable = result.gameObject.GetComponent<UnityEngine.UI.Selectable>();
            if (selectable != null && selectable.interactable)
            {
                return true; // We're over an interactable UI element
            }
        }

        return false; // We're over UI but not an interactable element
    }

    private void TrySelectUnit(Vector3 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        // Use sphere cast to make selection easier
        RaycastHit[] hits = Physics.SphereCastAll(ray, selectionRadius, raycastDistance);

        UnitController closestController = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            var controller = hit.transform.GetComponentInParent<UnitController>();

            // Only select units that are in the party
            if (controller != null &&
                partyService != null &&
                partyService.PartyControllers != null &&
                partyService.PartyControllers.Contains(controller))
            {
                // Find the closest unit if multiple are in range
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestController = controller;
                }
            }
        }

        if (closestController != null)
        {
            controlModeService.SelectUnit(closestController.Instance);
        }
    }

    private bool IsJoystickTouch(int touchIndex)
    {
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
}
