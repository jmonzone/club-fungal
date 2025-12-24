using UnityEngine;

public class CameraOrbitComponent : MonoBehaviour
{
    [Header("Orbit Settings")]
    public float orbitSensitivity = 0.2f;
    public float orbitSmoothTime = 0.06f;
    public float minPitch = 15f;
    public float maxPitch = 60f;
    public float minPitchClamp = -30f;
    public float maxPitchClamp = 80f;

    [Header("References")]
    public CameraService cameraService;

    [Header("Pokémon GO Effect")]
    [Range(0f, 1f)]
    [Tooltip("Blend between manual pitch and auto pitch determined by zoom/dolly. 0 = only manual pitch, 1 = only auto (zoom driven)")]
    public float pogoEffectAmount = 0.7f;

    [Header("Read Only")]
    [SerializeField][ReadOnly] float targetYaw;
    [SerializeField][ReadOnly] float yaw;
    [SerializeField][ReadOnly] float yawVel;

    [SerializeField][ReadOnly] float targetPitch;
    [SerializeField][ReadOnly] float pitch;
    [SerializeField][ReadOnly] float pitchVel;

    public void Initialize(float initialYaw, float initialPitch)
    {
        yaw = targetYaw = initialYaw;
        pitch = targetPitch = Mathf.Clamp(initialPitch, minPitchClamp, maxPitchClamp);

        if (cameraService != null)
        {
        }
    }

    private void OnEnable()
    {
        cameraService.OnYawDeltaRequested += OnYawDeltaRequested;
        cameraService.OnPitchDeltaRequested += OnPitchDeltaRequested;
    }

    private void OnDisable()
    {
        cameraService.OnYawDeltaRequested -= OnYawDeltaRequested;
        cameraService.OnPitchDeltaRequested -= OnPitchDeltaRequested;
    }

    private void OnYawDeltaRequested(float delta)
    {
        targetYaw += delta * orbitSensitivity;
    }

    private void OnPitchDeltaRequested(float delta)
    {
        targetPitch += delta * orbitSensitivity;
        targetPitch = Mathf.Clamp(targetPitch, minPitchClamp, maxPitchClamp);
    }

    public void AddPitch(float delta)
    {
        targetPitch += delta * orbitSensitivity;
        targetPitch = Mathf.Clamp(targetPitch, minPitchClamp, maxPitchClamp);
    }

    public void ApplyRotation(float currentDistance, float minDistance, float maxDistance)
    {
        // Smooth yaw
        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVel, orbitSmoothTime);

        // Auto-pitch derived from zoom/dolly
        float zoomT = Mathf.InverseLerp(minDistance, maxDistance, currentDistance);
        float autoPitch = Mathf.Lerp(minPitch, maxPitch, zoomT);

        // Blend manual targetPitch and autoPitch by pogoEffectAmount
        float blendedTargetPitch = Mathf.Lerp(targetPitch, autoPitch, pogoEffectAmount);

        // Smooth pitch toward blended target
        pitch = Mathf.SmoothDampAngle(pitch, blendedTargetPitch, ref pitchVel, orbitSmoothTime);

        // Apply rotation to rig
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
