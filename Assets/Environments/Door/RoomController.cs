using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [SerializeField] private GameObject ceilingObject;
    [SerializeField] private bool startAsInnerRoom = false;
    private List<WallController> walls = new List<WallController>();

    private void Awake()
    {
        walls.AddRange(GetComponentsInChildren<WallController>());
    }

    private void Start()
    {
        if (startAsInnerRoom)
            SetAsInnerRoom();
        else
            SetAsOuterRoom();
    }

    public void SetAsOuterRoom()
    {
        ceilingObject.SetActive(true);
        foreach (var wall in walls)
        {
            wall.SetAsOuterWall();
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
