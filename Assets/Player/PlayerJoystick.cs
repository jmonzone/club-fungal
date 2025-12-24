using UnityEngine;

public interface IJoystickSelectable
{
    void Select(UnitController source);
}

public class PlayerJoystick : MonoBehaviour
{
    [SerializeField] private PlayerService playerReference;
    [SerializeField] private VirtualJoystick virtualJoystick;
    [SerializeField] private InteractionController interaction;
    [SerializeField] private CameraService cameraService;

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

            // Rotate camera based on horizontal joystick input only
            if (cameraService != null && cameraService.CameraMode == CameraMode.THIRD_PERSON && Mathf.Abs(direction.x) > 0.01f)
            {
                cameraService.AddYaw(direction.x * 0.5f);
            }

            // Compute target position
            var targetPosition = playerReference.Player.transform.position + moveDir.normalized;

            // Detect IJoystickSelectable at target position
            Collider[] hitColliders = Physics.OverlapSphere(targetPosition, 0.5f);
            foreach (var hitCollider in hitColliders)
            {
                var interactable = hitCollider.GetComponentInParent<IJoystickSelectable>();
                if (interactable != null)
                {
                    interactable.Select(playerReference.Player);
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
