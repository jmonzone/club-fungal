using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoomController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private UnitListReference unitCollection;
    [SerializeField] private ActivityCollection activityCollection;

    [Header("References")]
    [SerializeField] private GameObject ceilingObject;
    [SerializeField] private Renderer floor;
    [SerializeField] private Transform activityAnchor;

    [Header("Runtime")]
    [SerializeField] private List<WallController> walls = new List<WallController>();

    public List<WallController> Walls => walls;
    public Renderer Floor => floor;

    public event UnityAction<DoorController> OnDoorSelected;

    private void OnValidate()
    {
        InitializeWalls();
    }

    private void Awake()
    {
        InitializeWalls();

        foreach (var door in GetComponentsInChildren<DoorController>(includeInactive: true))
        {
            door.OnDoorSelected += () => OnDoorSelected?.Invoke(door);
        }
    }

    private void InitializeWalls()
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

        unitCollection.SpawnNewUnit(unit => unit.Moves.Count > 0, activityAnchor.position, onSpawned: unit =>
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

    public void ActivateDoor(DoorController door)
    {
        if (door != null && !door.gameObject.activeSelf)
        {
            door.gameObject.SetActive(true);
        }
    }

    public void SetAsInnerRoom()
    {
#if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = gameObject;
#endif
        ceilingObject.SetActive(false);
        foreach (var wall in walls)
        {
            wall.SetAsInnerWall();
        }
    }
}
