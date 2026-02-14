using UnityEngine;

public class NetworkRunInitializer : MonoBehaviour
{
    [SerializeField] private NetworkRunService networkRunService;

    private void Awake()
    {
        networkRunService.Initialize();
    }
}
