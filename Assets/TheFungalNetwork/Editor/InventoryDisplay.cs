using UnityEditor;
using UnityEngine;
using System.Linq;

namespace TheFungalNetwork.Editor
{
    public class InventoryDisplay : CardDrawerDisplayItem
    {
        public InventoryDisplay(UnitInstance unitInstance, GUIStyle jobStyle)
        {
            condition = () => unitInstance?.Inventory?.ItemStacks != null && unitInstance.Inventory.ItemStacks.Count > 0;
            color = new Color(1f, 0.95f, 0.8f);
            drawAction = () =>
            {
                var itemCounts = unitInstance.Inventory.ItemStacks
                    .Where(stack => stack?.item != null)
                    .Select(stack => $"{stack.count}x {stack.item.DisplayName}")
                    .ToList();

                var inventoryText = string.Join(", ", itemCounts);
                EditorGUILayout.LabelField($"📦 {inventoryText}", jobStyle);
            };
        }
    }
}
