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

    [Header("Haptic Settings")]
    [SerializeField] private bool enableHaptics = true;
    [SerializeField] private float rotationHapticThreshold = 45f; // Degrees of rotation change to trigger haptic
    [SerializeField] private float rotationHapticCooldown = 0.15f; // Minimum time between rotation haptics

    private Camera mainCamera;
    private Vector3 lastJoystickDirection;
    private float lastRotationHapticTime;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Initialize iOS haptics
        iOSHaptics.Initialize();

        virtualJoystick.OnJoystickStart += position =>
        {
            interaction.Unselect();

            // Haptic feedback on joystick touch start
            if (enableHaptics)
            {
                iOSHaptics.Trigger(iOSHaptics.HapticStyle.Light, 0.6f);
            }

            lastJoystickDirection = Vector3.zero;
            lastRotationHapticTime = Time.time;
        };

        virtualJoystick.OnJoystickUpdate += direction =>
        {
            // Map joystick to XZ plane
            direction.z = direction.y;
            direction.y = 0;

            // Haptic feedback on rotation (direction change)
            if (enableHaptics && lastJoystickDirection.sqrMagnitude > 0.01f)
            {
                float angle = Vector3.Angle(lastJoystickDirection, direction);
                float timeSinceLastHaptic = Time.time - lastRotationHapticTime;

                if (angle > rotationHapticThreshold && timeSinceLastHaptic >= rotationHapticCooldown)
                {
                    // Vary intensity based on joystick displacement (further = stronger)
                    float displacement = direction.magnitude;
                    float intensity = Mathf.Lerp(0.3f, 0.7f, displacement);

                    iOSHaptics.Trigger(iOSHaptics.HapticStyle.Soft, intensity);
                    lastRotationHapticTime = Time.time;
                }
            }

            lastJoystickDirection = direction;

            // Get camera forward/right on XZ plane
            Vector3 camForward = mainCamera.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = mainCamera.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            // Rotate joystick input to be relative to camera
            Vector3 moveDir = camForward * direction.z + camRight * direction.x;

            // Always feed directional input to first ability (so it knows joystick direction for activation)
            if (playerReference.Player != null && playerReference.Player.Instance != null)
            {
                var abilities = playerReference.Player.Instance.Abilities;
                if (abilities != null && abilities.Count > 0)
                {
                    var ability = abilities[0];
                    if (ability != null)
                    {
                        ability.ApplyDirectionalInput(moveDir);

                        // If ability is controlling movement, don't do normal movement
                        if (ability.IsControllingMovement)
                        {
                            return;
                        }
                    }
                }
            }

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

            // Haptic feedback on joystick release
            if (enableHaptics)
            {
                iOSHaptics.Trigger(iOSHaptics.HapticStyle.Light, 0.5f);
            }

            lastJoystickDirection = Vector3.zero;
        };
    }

}
