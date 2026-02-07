using UnityEditor;
using UnityEngine;
using System.Linq;

namespace TheFungalNetwork.Editor
{
    public class InventoryDisplay : UnitDrawerDisplayItem
    {
        public InventoryDisplay(UnitInstance unitInstance, GUIStyle jobStyle)
        {
            condition = () => unitInstance?.Inventory?.Items != null && unitInstance.Inventory.Items.Count > 0;
            color = new Color(1f, 0.95f, 0.8f);
            drawAction = () =>
            {
                var itemCounts = unitInstance.Inventory.Items
                    .GroupBy(item => item)
                    .Select(g => $"{g.Count()}x {g.Key?.DisplayName ?? "Unknown"}")
                    .ToList();

                var inventoryText = string.Join(", ", itemCounts);
                EditorGUILayout.LabelField($"📦 {inventoryText}", jobStyle);
            };
        }
    }
}
