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
    [SerializeField] private RoomController roomControllerPrefab;
    [SerializeField] private List<Texture> wallTextures;

    [Header("Runtime")]
    [SerializeField] private List<RoomController> roomControllers;

    // Grid to track room positions using integer grid coordinates
    private Dictionary<Vector3Int, RoomController> roomGrid = new Dictionary<Vector3Int, RoomController>();

    private void Awake()
    {
        roomControllers = new List<RoomController>(GetComponentsInChildren<RoomController>());
        foreach (var room in roomControllers)
        {
            room.OnDoorSelected += door => OnDoorSelected(door);
            roomGrid[WorldToGrid(room.transform.position)] = room;
        }
    }

    //todo: not always create a new room, sometimes use an existing one
    private void OnDoorSelected(DoorController door)
    {
        // Check if the door already has an otherDoor assigned
        if (!door.OtherDoor)
        {
            var targetPosition = GetNewRoomPosition(door.Room.transform.position, door.transform.position);
            var targetRotation = door.Room.transform.rotation;
            var newRoom = Instantiate(roomControllerPrefab, targetPosition, targetRotation, transform);
            var randomtexture = wallTextures[Random.Range(0, wallTextures.Count)];
            newRoom.SetTexture(randomtexture);
            newRoom.name = $"Room_{roomControllers.Count}";
            newRoom.OnDoorSelected += door => OnDoorSelected(door);
            roomControllers.Add(newRoom);
            roomGrid[WorldToGrid(targetPosition)] = newRoom;

            var oppositeSide = door.Wall.Direction switch
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

            Debug.Log($"Linking door from {door.Room.name} to {newRoom.name} via walls {door.Wall.Direction} and {oppositeSide}");

            var oppositeDoor = newRoom.Walls[wallIndex].DoorController;
            door.SetOtherDoor(oppositeDoor);
            oppositeDoor.SetOtherDoor(door);
            newRoom.ActivateDoor(oppositeDoor);

            // Activate a random new door in addition to the opposite door, only if no room exists at the target position
            var candidateDoors = new List<DoorController>();
            foreach (var wall in newRoom.Walls)
            {
                if (wall.DoorController == oppositeDoor) continue;
                var candidatePos = GetNewRoomPosition(newRoom.transform.position, wall.DoorController.transform.position);
                if (!roomGrid.ContainsKey(WorldToGrid(candidatePos)))
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

        var teleportDirection = (door.OtherDoor.Room.transform.position - door.OtherDoor.transform.position).normalized;
        var teleportPosition = door.OtherDoor.transform.position + teleportDirection * 2f;
        playerReference.TeleportPlayer(teleportPosition);

        door.Room.SetAsOuterRoom();
        door.OtherDoor.Room.SetAsInnerRoom();
    }

    private const float RoomOffset = 15f;

    // Converts a world position to a grid position for reliable dictionary keys
    private Vector3Int WorldToGrid(Vector3 position)
    {
        // Use Mathf.RoundToInt for robustness
        return new Vector3Int(
            Mathf.RoundToInt(position.x / RoomOffset),
            Mathf.RoundToInt(position.y / RoomOffset),
            Mathf.RoundToInt(position.z / RoomOffset)
        );
    }

    private Vector3 GetNewRoomPosition(Vector3 roomPosition, Vector3 doorPosition)
    {
        var direction = doorPosition - roomPosition;
        var offset = RoomOffset * direction.normalized;
        return roomPosition + offset;
    }
}
