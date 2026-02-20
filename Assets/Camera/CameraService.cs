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

    [Header("Settings")]
    [SerializeField] private float settleDuration = 0.5f;

    public CameraMode CameraMode => cameraMode;
    public bool IsMoving => isMoving;
    public bool HasSettled => hasSettled;

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

    public void ReportMovement(bool moving)
    {
        if (moving)
        {
            isMoving = true;
            hasSettled = false;
            timeSinceLastMovement = 0f;
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
                OnCameraSettled?.Invoke();
            }
        }
    }
}
