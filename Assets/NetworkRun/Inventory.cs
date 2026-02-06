using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
    [SerializeField] private List<ItemTemplate> items = new List<ItemTemplate>();

    public List<ItemTemplate> Items => items;

    public void AddItem(ItemTemplate item)
    {
        items.Add(item);
    }

    public void RemoveItem(ItemTemplate item)
    {
        items.Remove(item);
    }

    public int GetItemCount(ItemTemplate itemTemplate)
    {
        int count = 0;
        foreach (var item in items)
        {
            if (item == itemTemplate)
            {
                count++;
            }
        }
        return count;
    }

    public int GetItemCountById(string id)
    {
        int count = 0;
        foreach (var item in items)
        {
            if (item != null && item.Id == id)
            {
                count++;
            }
        }
        return count;
    }

    public List<ItemTemplate> GetItemsByType(string id)
    {
        var result = new List<ItemTemplate>();
        foreach (var item in items)
        {
            if (item != null && item.Id == id)
            {
                result.Add(item);
            }
        }
        return result;
    }
}
