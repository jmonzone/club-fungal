using UnityEngine;

public interface IJoystickSelectable
{
    void Select();
}

public class PlayerJoystick : MonoBehaviour
{
    [SerializeField] private PlayerReference playerReference;
    [SerializeField] private VirtualJoystick virtualJoystick;
    [SerializeField] private InteractionController interaction;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        virtualJoystick.OnJoystickStart += position =>
        {
            interaction.Unselect();
        };

        virtualJoystick.OnJoystickUpdate += direction =>
        {
            // Map joystick to XZ plane
            direction.z = direction.y;
            direction.y = 0;

            // Get camera forward/right on XZ plane
            Vector3 camForward = mainCamera.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = mainCamera.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            // Rotate joystick input to be relative to camera
            Vector3 moveDir = camForward * direction.z + camRight * direction.x;

            // Compute target position
            var targetPosition = playerReference.Player.transform.position + moveDir.normalized;

            // Detect IJoystickSelectable at target position
            Collider[] hitColliders = Physics.OverlapSphere(targetPosition, 0.5f);
            foreach (var hitCollider in hitColliders)
            {
                var interactable = hitCollider.GetComponentInParent<IJoystickSelectable>();
                if (interactable != null)
                {
                    interactable.Select();
                    break;
                }
            }

            // Move player
            playerReference.SetTargetPosition(targetPosition);
        };

        virtualJoystick.OnJoystickEnd += () =>
        {
            playerReference.SetTargetPosition(playerReference.TargetPosition);
        };
    }

}
