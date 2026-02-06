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

            var shortcuts = new List<UnitDrawerItemAction>
            {
                new ActivityItemAction(
                    text: "Add 50 Spores",
                    emoji: "➕",
                    action: () =>
                    {
                        currentRun.AddSpores(50);
                    }
                ),
                new ActivityItemAction(
                    text: "Subtract 50 Spores",
                    emoji: "➖",
                    action: () =>
                    {
                        currentRun.SpendSpores(50);
                    }
                )
            };

            ItemDrawer.DrawItem(
                icon: null,
                displayName: "Spores",
                subtitle: currentRun.Spores.ToString(),
                backgroundColor: Color.white,
                shortcuts: shortcuts,
                menuItems: null,
                displayItems: null
            );
        }
    }
}
#endif
