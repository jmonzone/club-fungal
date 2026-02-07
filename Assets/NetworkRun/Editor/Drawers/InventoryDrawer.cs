#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class InventoryDrawer
    {
        public void Draw(NetworkRun currentRun)
        {
            if (currentRun == null) return;

            var items = currentRun.Inventory.Items;
            if (items == null || items.Count == 0)
            {
                EditorGUILayout.LabelField("  No items in inventory");
                return;
            }

            var itemGroups = new System.Collections.Generic.Dictionary<ItemTemplate, int>();
            foreach (var item in items)
            {
                if (item != null)
                {
                    if (itemGroups.ContainsKey(item))
                    {
                        itemGroups[item]++;
                    }
                    else
                    {
                        itemGroups[item] = 1;
                    }
                }
            }

            foreach (var kvp in itemGroups)
            {
                DrawItem(currentRun, kvp.Key, kvp.Value);
            }
        }

        private void DrawItem(NetworkRun currentRun, ItemTemplate itemTemplate, int count)
        {
            var shortcuts = new List<UnitDrawerItemAction>
            {
                new ActivityItemAction(
                    text: "Add Item",
                    emoji: "➕",
                    action: () =>
                    {
                        currentRun.Inventory.AddItem(itemTemplate);
                    }
                ),
                new ActivityItemAction(
                    text: "Remove Item",
                    emoji: "➖",
                    action: () =>
                    {
                        currentRun.Inventory.RemoveItem(itemTemplate);
                    }
                )
            };

            Texture icon = itemTemplate.Sprite?.texture;

            ItemDrawer.DrawItem(
                icon: icon,
                displayName: itemTemplate.DisplayName,
                subtitle: $"x{count}",
                backgroundColor: Color.white,
                shortcuts: shortcuts,
                menuItems: null,
                displayItems: null
            );
        }
    }
}
#endif
