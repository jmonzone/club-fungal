using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "InventoryViewReference", menuName = "Club Fungal/Views/Inventory View Reference")]
public class InventoryViewReference : ViewReference
{
    [SerializeField] private ItemInventory inventory;

    public event UnityAction OnInventoryUpdated
    {
        add => inventory.OnInventoryUpdated += value;
        remove => inventory.OnInventoryUpdated -= value;
    }


}