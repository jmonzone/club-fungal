using UnityEngine;

[RequireComponent(typeof(VirtualJoystick))]
public class VirtualJoystickWASD : MonoBehaviour
{
    private VirtualJoystick virtualJoystick;

    private void Awake()
    {
        virtualJoystick = GetComponent<VirtualJoystick>();
    }

    private void Update()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.magnitude > 0.1f)
        {
            if (!virtualJoystick.IsActive)
            {
                virtualJoystick.HandleJoystickStart(virtualJoystick.transform.position);
            }

            virtualJoystick.HandleJoystickUpdate(new Vector3(input.x, input.y, 0f) * 100f + virtualJoystick.transform.position);
        }
        else if (virtualJoystick.IsActive)
        {
            virtualJoystick.HandleJoystickRelease();
        }
    }
}
