using Cinemachine;
using UnityEngine;

public class ActivityCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private ActivityController activityController;

    private void Awake()
    {
        activityController = GetComponentInParent<ActivityController>();
    }

    private void OnEnable()
    {
        activityController.OnPlayerEnterEvent += OnPlayerEnter;
        activityController.OnPlayerExitEvent += OnPlayerExit;
    }

    private void OnDisable()
    {
        activityController.OnPlayerEnterEvent -= OnPlayerEnter;
        activityController.OnPlayerExitEvent -= OnPlayerExit;
    }

    private void OnPlayerEnter(ActivityUnit player)
    {
        virtualCamera.Priority = 11;
    }

    private void OnPlayerExit(ActivityUnit player)
    {
        virtualCamera.Priority = 0;
    }
}
