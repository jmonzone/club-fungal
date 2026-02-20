using UnityEngine;
using UnityEngine.Events;

public enum CameraMode
{
    FIRST_PERSON,
    THIRD_PERSON,
}

[CreateAssetMenu(fileName = "CameraService", menuName = "Club Fungal/Camera/Camera Service")]
public class CameraService : GURUService
{
    [Header("Runtime")]
    [SerializeField][ReadOnly] private CameraMode cameraMode;
    [SerializeField][ReadOnly] private bool isMoving;
    [SerializeField][ReadOnly] private bool hasSettled;
    [SerializeField][ReadOnly] private float timeSinceLastMovement;
    [SerializeField][ReadOnly] private float displacementFromSettled;
    [SerializeField][ReadOnly] private float partySpeedMultiplier = 1f;

    [Header("Settings")]
    [SerializeField] private float settleDuration = 0.5f;
    [SerializeField] private float unsettleDisplacementThreshold = 0.5f;
    [SerializeField][Range(0f, 2f)] private float partySpeedSensitivity = 1f;

    public CameraMode CameraMode => cameraMode;
    public bool IsMoving => isMoving;
    public bool HasSettled => hasSettled;
    public float PartySpeedMultiplier => Mathf.Lerp(1f, partySpeedMultiplier, partySpeedSensitivity);

    public event UnityAction<CameraMode> OnCameraModeChanged;
    public event UnityAction<float> OnYawDeltaRequested;
    public event UnityAction<float> OnPitchDeltaRequested;
    public event UnityAction OnCameraSettled;

    protected override void OnInitialize()
    {
        cameraMode = CameraMode.THIRD_PERSON;
        isMoving = false;
        hasSettled = false;
        timeSinceLastMovement = 0f;
        displacementFromSettled = 0f;
        partySpeedMultiplier = 1f;
    }

    public void SetPartySpeedMultiplier(float multiplier)
    {
        partySpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SetZoomT(float t)
    {
        CameraMode newMode = t < 0.01f ? CameraMode.FIRST_PERSON : CameraMode.THIRD_PERSON;
        if (newMode != cameraMode)
        {
            cameraMode = newMode;
            OnCameraModeChanged?.Invoke(cameraMode);
        }
    }

    public void AddYaw(float yawDelta)
    {
        OnYawDeltaRequested?.Invoke(yawDelta);
    }

    public void AddPitch(float pitchDelta)
    {
        OnPitchDeltaRequested?.Invoke(pitchDelta);
    }

    public void ReportMovement(float velocity, float displacement)
    {
        bool moving = velocity > 0.001f;
        displacementFromSettled = displacement;

        if (moving)
        {
            isMoving = true;
            timeSinceLastMovement = 0f;

            // Only unsettle if displacement exceeds threshold
            if (displacement >= unsettleDisplacementThreshold)
            {
                hasSettled = false;
            }
        }
        else
        {
            isMoving = false;
        }
    }

    public void UpdateSettleTimer(float deltaTime)
    {
        if (!isMoving)
        {
            timeSinceLastMovement += deltaTime;

            if (!hasSettled && timeSinceLastMovement >= settleDuration)
            {
                hasSettled = true;
                displacementFromSettled = 0f;
                OnCameraSettled?.Invoke();
            }
        }
    }
}
