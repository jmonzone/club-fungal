using UnityEngine;

public class WallController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject outerWallObject;
    [SerializeField] private GameObject innerWallObject;
    [SerializeField] private DoorController doorController;
    [SerializeField] private Renderer wallRenderer;

    [Header("Runtime")]
    [SerializeField] private Direction direction;

    public DoorController DoorController => doorController;
    public Direction Direction => direction;
    public Renderer Renderer => wallRenderer;

    public void SetAsOuterWall()
    {
        outerWallObject.SetActive(true);
        innerWallObject.SetActive(false);
    }

    public void SetAsInnerWall()
    {
        outerWallObject.SetActive(false);
        innerWallObject.SetActive(true);
    }

    public void SetDirection(Direction dir)
    {
        direction = dir;
    }
}
