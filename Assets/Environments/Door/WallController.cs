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
    [SerializeField] private Texture texture;

    public DoorController DoorController => doorController;
    public Direction Direction => direction;
    public Texture Texture => texture;

    private void OnValidate()
    {
        texture = wallRenderer.sharedMaterial.mainTexture;
    }

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

    public void SetTexture(Texture texture)
    {
        this.texture = texture;
        wallRenderer.sharedMaterial.mainTexture = texture;
    }

    public void SetDirection(Direction dir)
    {
        direction = dir;
    }
}
