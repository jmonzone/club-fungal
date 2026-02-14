using UnityEngine;

public class NetworkRunIslandManager : MonoBehaviour
{
    [SerializeField] private Transform treeTransform;
    private TreeController treeController;

    private void Awake()
    {
        treeController = treeTransform.GetComponent<TreeController>();
        var plantSporeEmitters = GetComponentsInChildren<PlantSporeEmitter>();

        foreach (var emitter in plantSporeEmitters)
        {
            emitter.SetTarget(treeTransform, OnSporeEmitted);
        }
    }

    private void OnSporeEmitted()
    {
        if (treeController != null)
        {
            treeController.AddSpore();
        }
    }
}
