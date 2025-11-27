using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "UnitControllerService", menuName = "Club Fungal/Units/Unit Controller Service")]
public class UnitControllerService : ScriptableObject
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
        if (unitControllers == null)
        {
            unitControllers = new List<UnitController>();
        }
    }

    public UnitController SpawnUnit(UnitInstance unit, Vector3 spawnPosition, Transform parent)
    {
        Debug.Log($"Spawning unit {unit.Data.Name} at position {spawnPosition}");
        var unitController = Instantiate(unitPrefab, spawnPosition, Quaternion.identity, parent);
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

    public void ClearControllers()
    {
        unitControllers.Clear();
    }
}
