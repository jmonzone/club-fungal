using UnityEngine;
using TMPro;

public class NetworkRunIslandManager : MonoBehaviour
{
    [SerializeField] private TreeController treeController;
    [SerializeField] private TextMeshProUGUI sporeCountText;

    private void Awake()
    {
        UpdateSporeCountText();

        var plantSporeEmitter = GetComponentInChildren<PlantSporeEmitter>();
        // if (plantSporeEmitter != null)
        // {
        //     plantSporeEmitter.OnSporeEmitted += OnSporeEmitted;
        // }
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
