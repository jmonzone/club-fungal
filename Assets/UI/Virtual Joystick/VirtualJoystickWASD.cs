using UnityEngine;

[RequireComponent(typeof(VirtualJoystick))]
public class VirtualJoystickWASD : MonoBehaviour
{
    private VirtualJoystick virtualJoystick;
    [SerializeField] private bool isActive = false;

    private void Awake()
    {
        virtualJoystick = GetComponent<VirtualJoystick>();
    }

    private void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.magnitude > 0.1f)
        {
            if (!isActive)
            {
                virtualJoystick.HandleJoystickStart(virtualJoystick.Rect.position);
                isActive = true;
            }
            virtualJoystick.HandleJoystickUpdate(new Vector3(input.x, input.y, 0f) * 100f + virtualJoystick.Rect.position);
        }
        else if (isActive)
        {
            isActive = false;
            virtualJoystick.HandleJoystickRelease();
        }
    }
}
