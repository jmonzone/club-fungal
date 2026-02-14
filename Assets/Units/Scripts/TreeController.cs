using Newtonsoft.Json.Linq;
using UnityEngine;


// <summary> A controller for tree units in the game. </summary>
public class TreeController : UnitController
{
    [Header("Scale Settings")]
    [SerializeField] private float minScale = 1f;
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private int maxSpores = 10;

    private int sporeCount = 0;
    private Vector3 baseScale;

    public int SporeCount => sporeCount;

    protected override void Awake()
    {
        base.Awake();
        baseScale = transform.localScale;
    }

    public void AddSpore()
    {
        sporeCount++;
        Debug.Log($"Tree now has {sporeCount} spores");
        UpdateScale();
    }

    private void UpdateScale()
    {
        float t = Mathf.Clamp01((float)sporeCount / maxSpores);
        float scale = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = baseScale * scale;
    }
}
