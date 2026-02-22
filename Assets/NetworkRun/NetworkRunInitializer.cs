using UnityEngine;

public class NetworkRunInitializer : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private CameraRotationController cameraController;

    private void Awake()
    {
        networkRunService.Initialize();
    }

    private void Start()
    {
        networkRunService.PartyService.SetSpawnParent(spawnParent);
        networkRunService.OnSceneLoaded();

        if (cameraController != null && networkRunService.PartyService.PartyLeader != null)
        {
            cameraController.target = networkRunService.PartyService.PartyLeader.transform;
        }
    }
}
