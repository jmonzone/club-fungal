using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceCost
{
    [SerializeField] private ItemTemplate item;
    [SerializeField] private int amount;
    
    public ItemTemplate Item => item;
    public int Amount => amount;
    
    public ResourceCost(ItemTemplate item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}

[Serializable]
public class UpgradeInstance
{
    [SerializeField] private Upgrade template;
    [SerializeField] private List<ResourceCost> price = new List<ResourceCost>();
    [SerializeField] private bool isPurchased = false;
    
    public Upgrade Template => template;
    public List<ResourceCost> Price => price;
    public bool IsPurchased => isPurchased;
    
    public UpgradeInstance(Upgrade template, List<ResourceCost> price)
    {
        this.template = template;
        this.price = price;
    }
    
    public bool CanAfford(Inventory inventory)
    {
        if (isPurchased || inventory == null || price == null)
            return false;
            
        foreach (var cost in price)
        {
            if (cost?.Item != null)
            {
                int availableCount = inventory.GetItemCount(cost.Item);
                if (availableCount < cost.Amount)
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    public void Purchase(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        if (isPurchased)
        {
            Debug.LogWarning($"Upgrade {template.UpgradeName} already purchased");
            return;
        }
        
        // Check if player has enough resources
        if (networkRun?.Inventory == null || !CanAfford(networkRun.Inventory))
        {
            Debug.LogWarning($"Not enough resources to purchase {template.UpgradeName}");
            return;
        }
        
        // Deduct resources from inventory
        foreach (var cost in price)
        {
            if (cost?.Item != null)
            {
                networkRun.Inventory.RemoveItem(cost.Item, cost.Amount);
                Debug.Log($"Deducted {cost.Amount}x {cost.Item.DisplayName} from inventory");
            }
        }
        
        // Apply the upgrade
        template.Apply(networkRun, activityInstance);
        isPurchased = true;
        
        Debug.Log($"Purchased upgrade: {template.UpgradeName}");
    }
}
