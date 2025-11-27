using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    NorthWest,
    NorthEast,
    SouthWest,
    SouthEast,
}

public class RoomManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerReference playerReference;
    [SerializeField] private TransitionReference transitionReference;
    [SerializeField] private RoomController roomControllerPrefab;
    [SerializeField] private List<Texture> wallTextureCollections;


    [Header("Runtime")]
    [SerializeField] private List<RoomController> roomControllers;
    [SerializeField] private bool isTransitioning = false;

    private Dictionary<Vector3Int, RoomController> roomGrid = new Dictionary<Vector3Int, RoomController>();

    private void Awake()
    {
        roomControllers = new List<RoomController>(GetComponentsInChildren<RoomController>());
        foreach (var room in roomControllers)
        {
            RegisterRoom(room);
        }
    }

    private void Start()
    {
        // wait for rooms to initialize
        foreach (var room in roomControllers)
        {
            foreach (var wall in room.Walls)
            {
                var door = wall.DoorController;
                if (!door || !door.gameObject.activeSelf) continue;

                var candidatePos = GetNewRoomPosition(room, door);
                if (roomGrid.ContainsKey(candidatePos))
                {
                    var otherRoom = roomGrid[candidatePos];
                    var oppositeDoor = GetOppositeDoor(wall.Direction, otherRoom);
                    door.SetOtherDoor(oppositeDoor);
                }
            }
        }
    }

    private void RegisterRoom(RoomController room)
    {
        room.OnDoorSelected += door => OnDoorSelected(door);
        var targetPosition = WorldToGrid(room.transform.localPosition);
        roomGrid[targetPosition] = room;
    }

    private void OnDoorSelected(DoorController door)
    {
        if (isTransitioning) return;
        isTransitioning = true;
        transitionReference.StartTransition(TransitionType.CIRCLE_IN, () =>
        {
            // Check if the door already has an otherDoor assigned
            if (!door.OtherDoor) CreateNewRoom(door);

            var teleportDirection = (door.OtherDoor.Room.transform.position - door.OtherDoor.transform.position).normalized;
            var teleportPosition = door.OtherDoor.transform.position + teleportDirection * 2f;
            playerReference.TeleportPlayer(teleportPosition);

            door.Room.SetAsOuterRoom();
            door.OtherDoor.Room.SetAsInnerRoom();

            transitionReference.StartTransition(TransitionType.CIRCLE_OUT, () =>
            {
                isTransitioning = false;
            });
        });
    }

    private void CreateNewRoom(DoorController door)
    {
        var targetPosition = GetNewRoomPosition(door.Room, door);
        var targetRotation = door.Room.transform.rotation;
        var newRoom = Instantiate(roomControllerPrefab, transform);
        newRoom.transform.localPosition = targetPosition;
        newRoom.transform.rotation = targetRotation;

        var randomMaterial = wallTextureCollections[Random.Range(0, wallTextureCollections.Count)];
        newRoom.Initialize(randomMaterial);
        newRoom.name = $"Room_{roomControllers.Count}";
        RegisterRoom(newRoom);
        roomControllers.Add(newRoom);

        var oppositeDoor = GetOppositeDoor(door.Wall.Direction, newRoom);

        door.SetOtherDoor(oppositeDoor);
        oppositeDoor.SetOtherDoor(door);
        newRoom.ActivateDoor(oppositeDoor);

        // Activate a random new door in addition to the opposite door, only if no room exists at the target position
        var candidateDoors = new List<DoorController>();
        foreach (var wall in newRoom.Walls)
        {
            if (wall.DoorController == oppositeDoor) continue;
            var candidatePos = GetNewRoomPosition(newRoom, wall.DoorController);
            if (!roomGrid.ContainsKey(candidatePos))
            {
                candidateDoors.Add(wall.DoorController);
            }
        }
        if (candidateDoors.Count > 0)
        {
            var randomDoor = candidateDoors[Random.Range(0, candidateDoors.Count)];
            newRoom.ActivateDoor(randomDoor);
        }
        else
        {
            Debug.LogWarning("No candidate doors available for additional activation.");
        }
    }

    private DoorController GetOppositeDoor(Direction direction, RoomController otherRoom)
    {
        var oppositeSide = direction switch
        {
            Direction.NorthEast => Direction.SouthWest,
            Direction.NorthWest => Direction.SouthEast,
            Direction.SouthEast => Direction.NorthWest,
            Direction.SouthWest => Direction.NorthEast,
            _ => throw new System.ArgumentOutOfRangeException()
        };

        var wallIndex = oppositeSide switch
        {
            Direction.NorthWest => 0,
            Direction.NorthEast => 1,
            Direction.SouthWest => 2,
            Direction.SouthEast => 3,
            _ => throw new System.ArgumentOutOfRangeException()
        };

        return otherRoom.Walls[wallIndex].DoorController;
    }

    private const float RoomOffset = 15f;

    private Vector3Int WorldToGrid(Vector3 position)
    {
        return new Vector3Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y),
            Mathf.RoundToInt(position.z)
        );
    }
    private Vector3Int GetNewRoomPosition(RoomController room, DoorController door)
    {
        var direction = door.transform.position - room.transform.position;
        var offset = Quaternion.Inverse(room.transform.rotation) * (RoomOffset * direction.normalized);
        var position = room.transform.localPosition + offset;

        return WorldToGrid(position);
    }
}
