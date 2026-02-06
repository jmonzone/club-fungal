using UnityEngine;

public class NetworkRun
{
    private RoomInstance currentRoom;
    private Inventory inventory;

    public Inventory Inventory => inventory;

    public NetworkRun(RoomInstance startingRoom)
    {
        currentRoom = startingRoom;
        inventory = new Inventory();
    }

}
