using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ScriptableObject definition for an inventory component.
/// Define inventory capacity and settings.
/// </summary>
[CreateAssetMenu(fileName = "InventoryComponent", menuName = "Club Fungal/Units/Components/Inventory Component")]
public class InventoryComponentDefinition : UnitComponentDefinition
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxCapacity = 20;
    [SerializeField] private bool allowStacking = true;
    [SerializeField] private int maxStackSize = 99;

    public int MaxCapacity => maxCapacity;
    public bool AllowStacking => allowStacking;
    public int MaxStackSize => maxStackSize;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new InventoryComponentInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of inventory component.
/// Manages item storage for a unit.
/// </summary>
[System.Serializable]
public class InventoryComponentInstance : UnitComponentInstance
{
    [System.Serializable]
    public class ItemStack
    {
        public string itemId;
        public int quantity;

        public ItemStack(string itemId, int quantity)
        {
            this.itemId = itemId;
            this.quantity = quantity;
        }
    }

    [SerializeField] private List<ItemStack> items = new List<ItemStack>();

    public InventoryComponentDefinition InventoryDefinition => definition as InventoryComponentDefinition;
    public List<ItemStack> Items => items;
    public int ItemCount => items.Count;
    public bool IsFull => ItemCount >= InventoryDefinition.MaxCapacity;

    public event UnityAction<string, int> OnItemAdded;
    public event UnityAction<string, int> OnItemRemoved;

    public InventoryComponentInstance(InventoryComponentDefinition definition, UnitController controller) 
        : base(definition, controller)
    {
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        items.Clear();
    }

    public bool AddItem(string itemId, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemId)) return false;

        // Try to stack if allowed
        if (InventoryDefinition.AllowStacking)
        {
            var existingStack = items.Find(i => i.itemId == itemId);
            if (existingStack != null)
            {
                int canAdd = Mathf.Min(quantity, InventoryDefinition.MaxStackSize - existingStack.quantity);
                if (canAdd > 0)
                {
                    existingStack.quantity += canAdd;
                    OnItemAdded?.Invoke(itemId, canAdd);
                    
                    // If we couldn't add all, try to add remaining to new stack
                    if (canAdd < quantity)
                    {
                        return AddItem(itemId, quantity - canAdd);
                    }
                    return true;
                }
            }
        }

        // Add new stack if not full
        if (!IsFull)
        {
            int addQuantity = InventoryDefinition.AllowStacking 
                ? Mathf.Min(quantity, InventoryDefinition.MaxStackSize) 
                : 1;
            
            items.Add(new ItemStack(itemId, addQuantity));
            OnItemAdded?.Invoke(itemId, addQuantity);
            
            // If we couldn't add all, try to add remaining
            if (addQuantity < quantity)
            {
                return AddItem(itemId, quantity - addQuantity);
            }
            return true;
        }

        return false; // Inventory full
    }

    public bool RemoveItem(string itemId, int quantity = 1)
    {
        if (string.IsNullOrEmpty(itemId)) return false;

        var stack = items.Find(i => i.itemId == itemId);
        if (stack == null) return false;

        int removeQuantity = Mathf.Min(quantity, stack.quantity);
        stack.quantity -= removeQuantity;
        OnItemRemoved?.Invoke(itemId, removeQuantity);

        if (stack.quantity <= 0)
        {
            items.Remove(stack);
        }

        // If we need to remove more, try the next stack
        if (removeQuantity < quantity)
        {
            return RemoveItem(itemId, quantity - removeQuantity);
        }

        return true;
    }

    public int GetItemCount(string itemId)
    {
        int total = 0;
        foreach (var stack in items)
        {
            if (stack.itemId == itemId)
            {
                total += stack.quantity;
            }
        }
        return total;
    }

    public bool HasItem(string itemId, int quantity = 1)
    {
        return GetItemCount(itemId) >= quantity;
    }

    public void Clear()
    {
        items.Clear();
    }
}
