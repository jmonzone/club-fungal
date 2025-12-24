using UnityEngine;

public class CameraFollowComponent : MonoBehaviour
{
    [Header("Follow Settings")]
    public Vector3 followOffset = Vector3.zero;
    public float followSmoothTime = 0.12f;

    [Header("Read Only")]
    [SerializeField][ReadOnly] Vector3 followVelocity;

    public void ApplyFollow(Transform target)
    {
        Vector3 desiredPos = target.position + followOffset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref followVelocity, followSmoothTime);
    }
}
