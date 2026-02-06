using UnityEngine;

public class CameraPanComponent : MonoBehaviour
{
    [Header("Pan Settings")]
    public float panSpeed = 0.5f;
    public float panSmoothTime = 0.12f;
    public float driftDamping = 0.92f;

    [Header("Read Only")]
    [SerializeField][ReadOnly] Vector3 panVelocity;
    [SerializeField][ReadOnly] Vector3 driftVelocity;

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
            driftVelocity *= driftDamping;
        }
        else
        {
            driftVelocity = Vector3.zero;
        }
    }

    public void AddPan(Vector2 delta)
    {
        Vector3 right = transform.right;
        Vector3 up = Vector3.up;
        driftVelocity += (right * delta.x + up * delta.y) * panSpeed;
    }
}
