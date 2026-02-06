using UnityEngine;

public class NetworkRun
{
    private RoomInstance currentRoom;
    private int spores;

    public int Spores => spores;

    public NetworkRun(RoomInstance startingRoom)
    {
        currentRoom = startingRoom;
        spores = 0;
    }

    public void AddSpores(int amount)
    {
        spores += amount;
    }

    public bool SpendSpores(int amount)
    {
        if (spores >= amount)
        {
            spores -= amount;
            return true;
        }
        return false;
    }

}
