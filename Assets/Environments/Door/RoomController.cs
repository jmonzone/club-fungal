using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class RoomController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private ActivityCollection activityCollection;

    [Header("References")]
    [SerializeField] private GameObject ceilingObject;
    [SerializeField] private Renderer floor;
    [SerializeField] private Transform activityAnchor;
    [SerializeField] private Transform unitParent;
    [SerializeField] private Transform buildParent;

    [Header("Settings")]
    [SerializeField] private int maxDoors = 2;
    public int MaxDoors => maxDoors;

    [Header("Runtime")]
    [SerializeField] private List<WallController> walls = new List<WallController>();

    public List<WallController> Walls => walls;
    public List<DoorController> Doors => walls.Where(wall => wall.DoorController.gameObject.activeSelf).Select(door => door.DoorController).ToList();
    public List<UnitController> Units => transform.GetComponentsInChildren<UnitController>(includeInactive: true).ToList();
    public Renderer Floor => floor;
    public Transform UnitParent => unitParent;
    public Transform BuildParent => buildParent;

    public event UnityAction<DoorController> OnDoorSelected;

    private void OnValidate()
    {
        InitializeComponents();
    }

    private void Awake()
    {
        InitializeComponents();

        foreach (var door in GetComponentsInChildren<DoorController>(includeInactive: true))
        {
            door.OnDoorSelected += () => OnDoorSelected?.Invoke(door);
        }
    }

    private void InitializeComponents()
    {
        walls = new List<WallController>(GetComponentsInChildren<WallController>());

        Direction[] directions = { Direction.NorthWest, Direction.NorthEast, Direction.SouthWest, Direction.SouthEast };
        for (var i = 0; i < walls.Count && i < directions.Length; i++)
        {
            walls[i].SetDirection(directions[i]);
        }
    }

    public void InitializeRandomActivity()
    {
        var randomActivity = activityCollection.GetRandomActivity();

        unitControllerService.SpawnNewUnit(unit => unit.Moves.Count > 0, activityAnchor.position, onSpawned: unit =>
        {
            randomActivity.StartActivity(activityAnchor.position, new List<UnitController> { unit });
        });
    }

    public void SetAsOuterRoom()
    {
        ceilingObject.SetActive(true);
        foreach (var wall in walls)
        {
            wall.SetAsOuterWall();
        }
    }

    [Obsolete("Should have wall controler be responsible for activating doors")]
    public void ActivateDoor(DoorController door)
    {
        if (door != null && !door.gameObject.activeSelf)
        {
            door.gameObject.SetActive(true);
        }
    }

    public void SetAsInnerRoom()
    {

        ceilingObject.SetActive(false);
        foreach (var wall in walls)
        {
            wall.SetAsInnerWall();
        }
    }

    public void AddUnit(UnitInstance unit)
    {
        unitControllerService.SpawnUnit(unit, activityAnchor.position, unitParent);
    }
}
