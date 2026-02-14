using UnityEngine;

public class NetworkRunInitializer : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private Transform spawnParent;

    private void Awake()
    {
        networkRunService.Initialize();
    }

    private void Start()
    {
        networkRunService.SetSpawnParent(spawnParent);
        networkRunService.OnSceneLoaded();

    }
}
