using UnityEngine;

public class CameraPanComponent : MonoBehaviour
{
    public enum PanAxisMode
    {
        XY,
        XZ
    }

    [Header("Pan Settings")]
    public float panSpeed = 0.5f;
    [Header("Pan Axis Mode")]
    public PanAxisMode panAxisMode = PanAxisMode.XY;
    public float touchDriftDamping = 0.85f;
    public float mouseDriftDamping = 0.92f;

    [Header("Party Speed")]
    [SerializeField] private CameraService cameraService;
    [SerializeField] private bool scaleWithPartySpeed = true;

    [Header("Read Only")]
    [SerializeField][ReadOnly] Vector3 driftVelocityValue;
    [SerializeField][ReadOnly] bool isTouch;
    [SerializeField][ReadOnly] float currentSpeedMultiplier = 1f;

    public Vector3 driftVelocity => driftVelocityValue;

    public void Initialize()
    {
        driftVelocityValue = Vector3.zero;
    }

    public void ApplyPan()
    {
        // Update speed multiplier from party
        if (scaleWithPartySpeed && cameraService != null)
        {
            currentSpeedMultiplier = cameraService.PartySpeedMultiplier;
        }
        else
        {
            currentSpeedMultiplier = 1f;
        }

        // Apply drift with damping
        if (driftVelocityValue.magnitude > 0.001f)
        {
            transform.position += driftVelocityValue * currentSpeedMultiplier * Time.deltaTime;
            float damping = isTouch ? touchDriftDamping : mouseDriftDamping;
            driftVelocityValue *= damping;
        }
        else
        {
            driftVelocityValue = Vector3.zero;
        }
    }

    public void AddPan(Vector2 delta, bool fromTouch = true)
    {
        isTouch = fromTouch;

        // Update speed multiplier
        float speedMultiplier = 1f;
        if (scaleWithPartySpeed && cameraService != null)
        {
            speedMultiplier = cameraService.PartySpeedMultiplier;
        }

        Vector3 right = transform.right;
        if (panAxisMode == PanAxisMode.XY)
        {
            Vector3 up = Vector3.up;
            driftVelocityValue += (right * delta.x + up * delta.y) * panSpeed * speedMultiplier;
        }
        else // XZ
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            driftVelocityValue += (right * delta.x + forward * delta.y) * panSpeed * speedMultiplier;
        }
    }
}
