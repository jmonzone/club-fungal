using UnityEngine;
using TMPro;

public class NetworkRunIslandManager : MonoBehaviour
{
    [SerializeField] private Transform treeTransform;
    [SerializeField] private TextMeshProUGUI sporeCountText;

    private TreeController treeController;

    private void Awake()
    {
        treeController = treeTransform.GetComponent<TreeController>();
        var plantSporeEmitters = GetComponentsInChildren<PlantSporeEmitter>();

        foreach (var emitter in plantSporeEmitters)
        {
            emitter.SetTarget(treeTransform, OnSporeEmitted);
        }

        UpdateSporeCountText();
    }

    private void OnSporeEmitted()
    {
        if (treeController != null)
        {
            treeController.AddSpore();
            UpdateSporeCountText();
        }
    }

    private void UpdateSporeCountText()
    {
        if (sporeCountText != null && treeController != null)
        {
            sporeCountText.text = $"{treeController.SporeCount}";
        }
    }
}
