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

    public CameraMode CameraMode => cameraMode;

    public event UnityAction<CameraMode> OnCameraModeChanged;
    public event UnityAction<float> OnYawDeltaRequested;
    public event UnityAction<float> OnPitchDeltaRequested;

    protected override void OnInitialize()
    {
        cameraMode = CameraMode.THIRD_PERSON;
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
}
