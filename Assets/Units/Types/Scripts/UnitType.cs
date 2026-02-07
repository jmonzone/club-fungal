using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a unit type with configurable properties like inventory capacity.
/// </summary>
[CreateAssetMenu(fileName = "New Unit Type", menuName = "Club Fungal/Units/Unit Type")]
public class UnitType : GURUObject
{
    [Serializable]
    public class ResourceSpeedBonus
    {
        public ItemTemplate resource; // The resource item
        public float speedBonus = 2f; // Speed multiplier when collecting this resource
    }

    [SerializeField] private int inventoryBonus = 0; // Bonus inventory slots added to base capacity
    [SerializeField] private List<ResourceSpeedBonus> resourceSpeedBonuses = new List<ResourceSpeedBonus>();

    public int InventoryBonus => inventoryBonus;
    public List<ResourceSpeedBonus> ResourceSpeedBonuses => resourceSpeedBonuses;

    public float GetSpeedBonusForResource(ItemTemplate item)
    {
        if (item == null || resourceSpeedBonuses == null)
            return 1f;

        foreach (var bonus in resourceSpeedBonuses)
        {
            if (bonus.resource == item)
            {
                return bonus.speedBonus;
            }
        }

        return 1f;
    }
}
