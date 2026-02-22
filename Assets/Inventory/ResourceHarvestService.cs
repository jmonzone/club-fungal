using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ResourceHarvestService", menuName = "Club Fungal/Resource Harvest Service")]
public class ResourceHarvestService : GURUService
{
    [Header("References")]
    [SerializeField] private InventoryReference inventoryReference;

    public event UnityAction<Item, Vector3, int> OnResourceHarvested;

    protected override void OnInitialize()
    {
        // Nothing to initialize
    }

    public void Harvest(Item item, int amount, Vector3 worldPosition)
    {
        inventoryReference.AddItem(item, amount);
        OnResourceHarvested?.Invoke(item, worldPosition, amount);
    }
}
