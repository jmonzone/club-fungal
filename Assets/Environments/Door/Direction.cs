using UnityEngine;

public class Direction
{
    public static readonly Direction NorthWest = new Direction(0, new Vector3(-1, 0, 0));
    public static readonly Direction NorthEast = new Direction(1, new Vector3(1, 0, 0));
    public static readonly Direction SouthWest = new Direction(2, new Vector3(0, 0, -1));
    public static readonly Direction SouthEast = new Direction(3, new Vector3(0, 0, -1));
    public int WallIndex { get; private set; }
    public Vector3 OffsetNormalized { get; private set; }
    public Direction Opposite { get; private set; }

    private Direction(int wallIndex, Vector3 offsetNormalized)
    {
        WallIndex = wallIndex;
        OffsetNormalized = offsetNormalized;
    }

    static Direction()
    {
        // Set opposite directions after all instances are created
        NorthWest.Opposite = SouthEast;
        NorthEast.Opposite = SouthWest;
        SouthWest.Opposite = NorthEast;
        SouthEast.Opposite = NorthWest;
    }

    public Vector3 GetOffset(float roomOffset)
    {
        return OffsetNormalized * roomOffset;
    }
}
