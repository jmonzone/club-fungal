using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "UnitControllerService", menuName = "Club Fungal/Units/Unit Controller Service")]
public class UnitControllerService : GURUService
{
    [Header("References")]
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private UnitController unitPrefab;

    [Header("Runtime")]
    [SerializeField] private List<UnitController> unitControllers;

    public List<UnitController> UnitControllers => unitControllers;

    public event UnityAction<UnitController> OnUnitSummoned;

    public void Initialize()
    {
        unitControllers = new List<UnitController>();
        UnitController[] controllers = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        unitControllers.AddRange(controllers);
    }

    public UnitController SpawnUnit(UnitInstance unit, Vector3 spawnPosition, Transform parent)
    {
        Debug.Log($"Spawning unit {unit.Data.Name} at position {spawnPosition}");
#if UNITY_EDITOR
        var unitController = PrefabUtility.InstantiatePrefab(unitPrefab, parent) as UnitController;
#else
        var unitController = Instantiate(unitPrefab, parent);
#endif
        unitController.transform.position = spawnPosition;
        unitController.transform.rotation = Quaternion.identity;
        unitController.Initialize(unit);
        Debug.Log($"Initialized unit controller for {unit.Data.Name} spawned at {unitController.transform.position}");

        unitControllers.Add(unitController);

        OnUnitSummoned?.Invoke(unitController);
        return unitController;
    }

    public event UnityAction<UnitController, UnitInstance> OnFriendInvited;

    public void InviteFriend(UnitController unit)
    {
        var friend = unitInstanceService.CreateNewFriend(unit.Instance);
        OnFriendInvited?.Invoke(unit, friend);
    }

    public void SpawnNewUnit(UnitInstanceService.UnitQuery unitQuery, Vector3 position, UnityAction<UnitController> onSpawned = null)
    {
        var unit = unitInstanceService.CreateUnit(unitQuery);
        var unitController = SpawnUnit(unit, position, null);
        onSpawned?.Invoke(unitController);
    }

    public void RemoveController(UnitController controller)
    {
        if (unitControllers.Contains(controller))
        {
            unitControllers.Remove(controller);
        }
    }

    public UnitController GetController(UnitInstance instance)
    {
        return unitControllers.Find(c => c.Instance == instance);
    }
}
