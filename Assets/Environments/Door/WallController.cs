using UnityEngine;

public class WallController : MonoBehaviour
{
    [SerializeField] private GameObject outerWallObject;
    [SerializeField] private GameObject innerWallObject;

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
}
