using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Inventory
{
    [SerializeField] private List<ItemStack> itemStacks = new List<ItemStack>();

    [Serializable]
    public class ItemStack
    {
        public ItemTemplate item;
        public int count;

        public ItemStack(ItemTemplate item, int count)
        {
            this.item = item;
            this.count = count;
        }
    }

    public List<ItemStack> ItemStacks => itemStacks;

    public void AddItem(ItemTemplate item)
    {
        var existingStack = itemStacks.FirstOrDefault(s => s.item == item);
        if (existingStack != null)
        {
            existingStack.count++;
        }
        else
        {
            itemStacks.Add(new ItemStack(item, 1));
        }
        // Debug.Log($"[Inventory] Added item: {(item != null ? item.DisplayName : "NULL")}. Total unique items: {itemStacks.Count}");
    }

    public void RemoveItem(ItemTemplate item, int count = 1)
    {
        var existingStack = itemStacks.FirstOrDefault(s => s.item == item);
        if (existingStack != null)
        {
            existingStack.count -= count;
            if (existingStack.count <= 0)
            {
                itemStacks.Remove(existingStack);
            }
        }
    }

    public int GetItemCount(ItemTemplate itemTemplate)
    {
        var stack = itemStacks.FirstOrDefault(s => s.item == itemTemplate);
        int count = stack?.count ?? 0;
        // Debug.Log($"[Inventory] GetItemCount for {(itemTemplate != null ? itemTemplate.DisplayName : "NULL")}: {count} (Total unique items: {itemStacks.Count})");
        return count;
    }

    public int GetItemCountById(string id)
    {
        var stack = itemStacks.FirstOrDefault(s => s.item != null && s.item.Id == id);
        return stack?.count ?? 0;
    }

    public List<ItemTemplate> GetItemsByType(string id)
    {
        var result = new List<ItemTemplate>();
        var stack = itemStacks.FirstOrDefault(s => s.item != null && s.item.Id == id);
        if (stack != null)
        {
            for (int i = 0; i < stack.count; i++)
            {
                result.Add(stack.item);
            }
        }
        return result;
    }
}
