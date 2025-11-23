using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RoomController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private UnitListReference unitCollection;
    [SerializeField] private ActivityReference activityReference;

    [Header("References")]
    [SerializeField] private GameObject ceilingObject;
    [SerializeField] private Transform activityAnchor;
    private List<WallController> walls = new List<WallController>();

    public List<WallController> Walls => walls;

    public event UnityAction<DoorController> OnDoorSelected;

    private void Awake()
    {
        walls.AddRange(GetComponentsInChildren<WallController>());

        for (var i = 0; i < walls.Count; i++)
        {
            walls[i].SetDirection((Direction)i);
        }

        foreach (var door in GetComponentsInChildren<DoorController>(includeInactive: true))
        {
            door.OnDoorSelected += () => OnDoorSelected?.Invoke(door);
        }
    }

    public void Initialize(Texture texture)
    {
        unitCollection.SpawnNewUnit(unit => unit.Moves.Count > 0, activityAnchor.position, onSpawned: unit =>
        {
            activityReference.StartActivity(activityAnchor.position, new List<UnitController> { unit });
        });

        walls.ForEach(wall => wall.SetTexture(texture));
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
        ceilingObject.SetActive(false);
        foreach (var wall in walls)
        {
            wall.SetAsInnerWall();
        }
    }
}
