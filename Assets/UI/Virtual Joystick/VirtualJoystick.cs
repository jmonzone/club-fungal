using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool hideWhenInactive = false;

    [Header("References")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private RectTransform joystickRect;
    [SerializeField] private GameObject target;
    [SerializeField] private float sensitivity;

    [Header("Runtime")]
    [SerializeField] private bool isActive;
    [SerializeField] private Vector3 direction;
    public event UnityAction<Vector3> OnJoystickStart;
    public event UnityAction<Vector3> OnJoystickUpdate;
    public event UnityAction OnJoystickEnd;

    private const float JOYSTICK_LENGTH = 100f;

    private Vector3 defaultPosition;
    private Vector3 startPosition;

    public bool IsActive => isActive;

    private void Start()
    {
        defaultPosition = rect.position;

        if (hideWhenInactive)
        {
            rect.gameObject.SetActive(false);
        }
    }

    private int activeTouchIndex = -1; // Tracks the touch index (-1 for mouse)
    private bool IsTouch => activeTouchIndex >= 0;

    private void Update()
    {
        if (IsActive && IsInputReleased())
        {
            HandleJoystickRelease();
        }

        // Handle joystick start
        if (!IsActive && IsInputPressed(out int touchIndex))
        {
            activeTouchIndex = touchIndex;
            HandleJoystickStart(GetInputPosition());
        }

        // Handle joystick movement
        if (IsActive && IsInputHeld())
        {
            HandleJoystickUpdate(GetInputPosition());
        }
    }

    // Get input position for the active touch or mouse
    private Vector3 GetInputPosition()
    {
        if (IsTouch && activeTouchIndex < Input.touchCount)
            return Input.GetTouch(activeTouchIndex).position;
        return Input.mousePosition;
    }

    private bool IsInputPressed(out int touchIndex)
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began && IsPointerOverTarget(touch.position))
            {
                touchIndex = i;
                return true;
            }
        }

        // Mouse fallback for editor use
        if (Input.GetMouseButtonDown(0) && IsPointerOverTarget(Input.mousePosition))
        {
            touchIndex = -1;
            return true;
        }

        touchIndex = -2; // Nothing found
        return false;
    }


    // Check if the active input is being held
    private bool IsInputHeld()
    {
        if (IsTouch && activeTouchIndex < Input.touchCount)
        {
            var phase = Input.GetTouch(activeTouchIndex).phase;
            return phase == TouchPhase.Moved || phase == TouchPhase.Stationary;
        }
        return Input.GetMouseButton(0);
    }

    // Check if the active input has been released
    private bool IsInputReleased()
    {
        if (IsTouch && activeTouchIndex < Input.touchCount)
        {
            return Input.GetTouch(activeTouchIndex).phase == TouchPhase.Ended;
        }
        return Input.GetMouseButtonUp(0);
    }

    private bool IsPointerOverTarget(Vector2 position)
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        return !target || (results.Count > 0 && results[0].gameObject == target);
    }

    public void HandleJoystickStart(Vector3 inputPosition)
    {
        isActive = true;
        startPosition = inputPosition;
        rect.position = startPosition;

        if (hideWhenInactive)
        {
            rect.gameObject.SetActive(true);
        }

        OnJoystickStart?.Invoke(startPosition);
    }

    public void HandleJoystickUpdate(Vector3 inputPosition)
    {
        direction = Vector3.ClampMagnitude(inputPosition - startPosition, JOYSTICK_LENGTH);
        joystickRect.anchoredPosition = direction;
        OnJoystickUpdate?.Invoke(direction * sensitivity);
    }

    public void HandleJoystickRelease()
    {
        isActive = false;
        activeTouchIndex = -1;
        rect.position = defaultPosition;
        joystickRect.anchoredPosition = Vector3.zero;

        if (hideWhenInactive)
        {
            rect.gameObject.SetActive(false);
        }

        OnJoystickEnd?.Invoke();
    }

}