using Cinemachine;
using UnityEngine;

public class ActivityCameraController : MonoBehaviour
{
    [SerializeField] private PlayerActivityReference activity;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    protected virtual void OnEnable()
    {
        activity.OnPlayerEnter += OnPlayerEnter;
        activity.OnPlayerExit += OnPlayerExit;
    }

    protected virtual void OnDisable()
    {
        activity.OnPlayerEnter -= OnPlayerEnter;
        activity.OnPlayerExit -= OnPlayerExit;
    }

    protected void OnPlayerEnter(ActivityUnit player)
    {
        virtualCamera.Priority = 11;
    }

    protected void OnPlayerExit(ActivityUnit player)
    {
        virtualCamera.Priority = 0;
    }
}
