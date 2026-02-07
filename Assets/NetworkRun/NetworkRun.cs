using UnityEngine;

public class NetworkRun
{
    private RoomInstance currentRoom;
    private Inventory inventory;

    public Inventory Inventory => inventory;
    public RoomInstance CurrentRoom => currentRoom;

    public NetworkRun()
    {
        inventory = new Inventory();
    }

    public NetworkRun(RoomInstance startingRoom)
    {
        Debug.Log($"Starting new run in room: {startingRoom.Data.id}");
        currentRoom = startingRoom;
        inventory = new Inventory();
    }

    public void SetInventory(Inventory loadedInventory)
    {
        inventory = loadedInventory;
    }

    public void SetCurrentRoom(RoomInstance roomInstance)
    {
        currentRoom = roomInstance;
    }

}
