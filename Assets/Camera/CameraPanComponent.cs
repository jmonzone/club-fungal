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

    [Header("Read Only")]
    [SerializeField][ReadOnly] Vector3 driftVelocity;
    [SerializeField][ReadOnly] bool isTouch;

    public void Initialize()
    {
        driftVelocity = Vector3.zero;
    }

    public void ApplyPan()
    {
        // Apply drift with damping
        if (driftVelocity.magnitude > 0.001f)
        {
            transform.position += driftVelocity * Time.deltaTime;
            float damping = isTouch ? touchDriftDamping : mouseDriftDamping;
            driftVelocity *= damping;
        }
        else
        {
            driftVelocity = Vector3.zero;
        }
    }

    public void AddPan(Vector2 delta, bool fromTouch = true)
    {
        isTouch = fromTouch;
        Vector3 right = transform.right;
        if (panAxisMode == PanAxisMode.XY)
        {
            Vector3 up = Vector3.up;
            driftVelocity += (right * delta.x + up * delta.y) * panSpeed;
        }
        else // XZ
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            driftVelocity += (right * delta.x + forward * delta.y) * panSpeed;
        }
    }
}
