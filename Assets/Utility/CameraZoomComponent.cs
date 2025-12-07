using UnityEngine;
using Cinemachine;

public class CameraZoomComponent : MonoBehaviour
{
    [Header("Zoom / Dolly Settings")]
    public float minDistance = 2f;
    public float maxDistance = 12f;
    public float zoomSpeed = 5f;
    public float zoomSmoothTime = 0.12f;

    [Header("FOV Settings")]
    public float fovMin = 30f;
    public float fovMax = 60f;
    public float fovSmoothTime = 0.12f;

    [Header("References")]
    public CinemachineVirtualCamera vCam;
    public PlayerService playerReference;

    [Header("Read Only")]
    [SerializeField][ReadOnly] float targetDistance;
    [SerializeField][ReadOnly] float distance;
    [SerializeField][ReadOnly] float distanceVel;

    [SerializeField][ReadOnly] float targetHeight;
    [SerializeField][ReadOnly] float height;
    [SerializeField][ReadOnly] float heightVel;

    [SerializeField][ReadOnly] float targetFov;
    [SerializeField][ReadOnly] float currentFov;
    [SerializeField][ReadOnly] float fovVel;

    public float CurrentDistance => distance;

    public void Initialize()
    {
        if (vCam == null) return;

        float z = vCam.transform.localPosition.z;
        distance = targetDistance = Mathf.Abs(z) > 0.001f ? -z : (minDistance + maxDistance) * 0.5f;

        currentFov = vCam.m_Lens.FieldOfView;
        targetFov = Mathf.Lerp(fovMin, fovMax, Mathf.InverseLerp(minDistance, maxDistance, distance));
    }

    public void Zoom(float delta)
    {
        targetDistance -= delta;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        targetHeight -= delta;
        targetHeight = Mathf.Clamp(targetHeight, 0, 8);

        // Update targetFov mapped to distance
        float t = Mathf.InverseLerp(minDistance, maxDistance, targetDistance);
        if (playerReference != null) playerReference.SetCameraZoom(t);

        targetFov = Mathf.Lerp(fovMin, fovMax, t);
    }

    public void ApplyZoom()
    {
        if (vCam == null) return;

        // Smoothly approach the target distance
        distance = Mathf.SmoothDamp(distance, targetDistance, ref distanceVel, zoomSmoothTime);
        height = Mathf.SmoothDamp(height, targetHeight, ref heightVel, zoomSmoothTime);

        // Apply dolly by moving the child vCam along local Z
        Vector3 camLocal = vCam.transform.localPosition;
        camLocal.z = -distance;
        vCam.transform.localPosition = camLocal;

        // Smoothly update FOV
        currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovVel, fovSmoothTime);
        vCam.m_Lens.FieldOfView = currentFov;
    }
}
