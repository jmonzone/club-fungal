using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "UnitControllerService", menuName = "Club Fungal/Units/Unit Controller Service")]
public class UnitControllerService : GURUService
{
    public delegate void FriendInvitedDelegate(UnitController inviter, UnitController friend);

    [Header("References")]
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private FungalController fungalPrefab;
    [SerializeField] private TreeController treePrefab;
    [SerializeField] private PlayerController playerPrefab;

    [Header("Runtime")]
    [SerializeField] private List<UnitController> unitControllers;

    public List<UnitController> Controllers => unitControllers;
    public event UnityAction<UnitController> OnUnitSummoned;

    protected override void OnInitialize()
    {
        InitializeControllers();
    }

    public override void OnSceneLoaded()
    {
        InitializeControllers();
    }

    private void InitializeControllers()
    {
        Debug.Log("Initializing UnitControllers in scene...");
        unitControllers = new List<UnitController>();
        UnitController[] controllers = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        unitControllers.AddRange(controllers);

        var instancesById = unitInstanceService.Instances.ToDictionary(instance => instance.Id);
        InitializeExistingControllers(instancesById);

        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "Gameplay" || sceneName == "Club Fungal")
        {
            SpawnMissingControllers(instancesById);
        }
    }

    private void InitializeExistingControllers(Dictionary<string, UnitInstance> instancesById)
    {
        foreach (var controller in unitControllers)
        {
            if (instancesById.TryGetValue(controller.Instance.Id, out var matchingInstance))
            {
                // Debug.Log($"Initializing UnitController for instance ID: {matchingInstance.Id}");
                controller.Initialize(matchingInstance);
            }
        }
    }

    private void SpawnMissingControllers(Dictionary<string, UnitInstance> instancesById)
    {
        var controllerInstanceIds = new HashSet<string>(unitControllers.Select(c => c.Instance.Id));

        foreach (var instance in unitInstanceService.Instances)
        {
            if (!controllerInstanceIds.Contains(instance.Id))
            {
                SpawnUnit(instance, Vector3.zero, null);
            }
        }
    }

    public UnitController SpawnUnit(UnitInstance unit, Vector3 spawnPosition, Transform parent)
    {
        UnitController unitPrefab = unit.Species.Id switch
        {
            "tree" => treePrefab,
            "player" => playerPrefab,
            _ => fungalPrefab,
        };

#if UNITY_EDITOR
        UnitController unitController;
        unitController = PrefabUtility.InstantiatePrefab(unitPrefab, parent) as UnitController;
#else
        unitController = Instantiate(unitPrefab, parent);
#endif
        unitController.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        unitController.Initialize(unit);

        unitControllers.Add(unitController);

        OnUnitSummoned?.Invoke(unitController);
        return unitController;
    }

    public event FriendInvitedDelegate OnFriendInvited;

    public Vector3 GetRandomSpawnPosition(Vector3 startPosition)
    {
        var spawnPosition = startPosition + Random.insideUnitSphere.normalized * 2f;
        spawnPosition.y = startPosition.y;
        return spawnPosition;
    }

    public void InviteFriend(UnitController unit)
    {
        var friend = unitInstanceService.CreateNewFriend(unit.Instance);
        var spawnPosition = GetRandomSpawnPosition(unit.transform.position);

        // Always spawn immediately
        var spawnedUnit = SpawnUnit(friend, spawnPosition, null);

        // Notify for optional fancy effects
        OnFriendInvited?.Invoke(unit, spawnedUnit);
    }
    public void SpawnNewUnit(UnitInstanceService.UnitQuery unitQuery, Vector3 position, UnityAction<UnitController> onSpawned = null)
    {
        var unit = unitInstanceService.CreateUnit(unitQuery);
        var unitController = SpawnUnit(unit, position, null);
        onSpawned?.Invoke(unitController);
    }
}
