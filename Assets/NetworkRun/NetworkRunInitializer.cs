using UnityEngine;

public class NetworkRunInitializer : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private CameraRotationController cameraController;

    private void Start()
    {

        if (cameraController != null && networkRunService.PartyService.PartyLeader != null)
        {
            cameraController.target = networkRunService.PartyService.PartyLeader.transform;
        }

        networkRunService.PartyService.OnPartyLeaderChanged += UpdateCameraTarget;
    }

    private void OnDestroy()
    {
        if (networkRunService?.PartyService != null)
        {
            networkRunService.PartyService.OnPartyLeaderChanged -= UpdateCameraTarget;
        }
    }

    private void UpdateCameraTarget(UnitController newLeader)
    {
        if (cameraController != null && newLeader != null)
        {
            cameraController.target = newLeader.transform;
        }
    }
}
