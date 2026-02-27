using UnityEngine;

/// <summary>
/// Makes a unit collectible by nearby collectors.
/// Can add conditions like requiring stun/damage before collection.
/// </summary>
[CreateAssetMenu(fileName = "CollectibleComponent", menuName = "Club Fungal/Units/Components/Collectible Component")]
public class CollectibleComponentDefinition : UnitComponentDefinition
{
    [Header("Collection Settings")]
    [SerializeField] private Item item;
    [SerializeField] private int amount = 1;
    [SerializeField] private InventoryReference globalInventory;

    // [Header("Collection Conditions")]
    // [SerializeField] private bool requiresStun = false;
    // [SerializeField] private bool requiresDamage = false;
    // [SerializeField] private Item requiredItem; // Optional: need item in inventory to collect

    public Item Item => item;
    public int Amount => amount;
    public InventoryReference GlobalInventory => globalInventory;
    // public bool RequiresStun => requiresStun;
    // public bool RequiresDamage => requiresDamage;
    // public Item RequiredItem => requiredItem;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new CollectibleComponentInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of collectible component.
/// Manages collection logic and conditions.
/// </summary>
[System.Serializable]
public class CollectibleComponentInstance : UnitComponentInstance
{
    public CollectibleComponentDefinition CollectibleDefinition => definition as CollectibleComponentDefinition;

    public CollectibleComponentInstance(CollectibleComponentDefinition definition, UnitController controller)
        : base(definition, controller)
    {
    }

    /// <summary>
    /// Attempts to collect this unit and add item to inventory.
    /// Returns true if collection succeeded.
    /// </summary>
    public bool TryCollect(UnitController collectorController)
    {
        // // Check stun condition
        // if (CollectibleDefinition.RequiresStun)
        // {
        //     var stunnedComponent = controller.GetComponentInstance<StunnedComponentInstance>();
        //     if (stunnedComponent == null)
        //     {
        //         return false;
        //     }
        // }
        // 
        // // Check damage condition
        // if (CollectibleDefinition.RequiresDamage)
        // {
        //     if (controller.Health == null || controller.Health.HealthAmount >= controller.Health.MaxHealthAmount)
        //     {
        //         return false;
        //     }
        // }
        // 
        // // Check required item
        // if (CollectibleDefinition.RequiredItem != null)
        // {
        //     int itemCount = CollectibleDefinition.GlobalInventory?.GetItemCount(CollectibleDefinition.RequiredItem) ?? 0;
        //     if (itemCount <= 0)
        //     {
        //         return false;
        //     }
        // }

        // Add item to global inventory
        CollectibleDefinition.GlobalInventory.AddItem(CollectibleDefinition.Item, CollectibleDefinition.Amount);

        // // Consume required item if specified
        // if (CollectibleDefinition.RequiredItem != null)
        // {
        //     CollectibleDefinition.GlobalInventory.RemoveItem(CollectibleDefinition.RequiredItem, 1);
        // }

        // Disable the collectible unit (allows for pooling/respawning)
        controller.gameObject.SetActive(false);

        return true;
    }
}
