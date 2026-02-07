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

            var itemStacks = currentRun.Inventory.ItemStacks;
            if (itemStacks == null || itemStacks.Count == 0)
            {
                EditorGUILayout.LabelField("  No items in inventory");
                return;
            }

            foreach (var stack in itemStacks)
            {
                if (stack?.item != null)
                {
                    DrawItem(currentRun, stack.item, stack.count);
                }
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
