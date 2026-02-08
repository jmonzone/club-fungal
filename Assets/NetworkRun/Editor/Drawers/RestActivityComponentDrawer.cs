#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class RestActivityComponentDrawer : ActivityComponentDrawer<RestActivity>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();

        public override List<CardDrawerDisplayItem> GetDisplayItems(ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            if (component is RestActivity restActivity)
            {
                return new List<CardDrawerDisplayItem>
                {
                    new RestInventoryDisplayItem(restActivity, onChanged)
                };
            }
            return null;
        }

        protected override void DrawTypedUnitCard(UnitInstance unit, ActivityInstance activity, RestActivity component, NetworkRun currentRun, System.Action onChanged)
        {
            _cardDrawer.Draw(
                unit,
                () =>
                {
                    // Remove button (debug mode)
                    if (currentRun.Settings.debugMode)
                    {
                        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                        if (GUILayout.Button("Remove", GUILayout.Height(18), GUILayout.Width(90)))
                        {
                            activity.RemoveUnit(unit);
                            UnityEditor.AssetDatabase.SaveAssets();
                            onChanged?.Invoke();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                },
                null,
                () =>
                {
                    // Show transfer buttons
                    if (component?.Inventory != null && unit?.Inventory != null)
                    {
                        EditorGUILayout.Space(2);

                        // Unload All button
                        if (unit.Inventory.ItemStacks.Any())
                        {
                            GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
                            if (GUILayout.Button("⬇ Unload All", GUILayout.Height(20), GUILayout.Width(90)))
                            {
                                UnloadAllItems(unit, component);
                                onChanged?.Invoke();
                            }
                            GUI.backgroundColor = Color.white;
                        }

                        // Load All button
                        if (component.Inventory.ItemStacks.Any() && !unit.Inventory.IsFull)
                        {
                            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                            if (GUILayout.Button("⬆ Load All", GUILayout.Height(20), GUILayout.Width(90)))
                            {
                                LoadAllItems(unit, component);
                                onChanged?.Invoke();
                            }
                            GUI.backgroundColor = Color.white;
                        }

                        EditorGUILayout.Space(2);
                    }
                });
        }

        private void UnloadAllItems(UnitInstance unit, RestActivity restActivity)
        {
            var items = unit.Inventory.ItemStacks.ToList();
            foreach (var itemStack in items)
            {
                var item = itemStack.item;
                var count = itemStack.count;

                for (int i = 0; i < count; i++)
                {
                    unit.Inventory.RemoveItem(item, 1);
                    restActivity.Inventory.AddItem(item);
                }
            }
            UnityEditor.AssetDatabase.SaveAssets();
        }

        private void LoadAllItems(UnitInstance unit, RestActivity restActivity)
        {
            var items = restActivity.Inventory.ItemStacks.ToList();
            foreach (var itemStack in items)
            {
                var item = itemStack.item;
                var count = itemStack.count;

                for (int i = 0; i < count; i++)
                {
                    if (unit.Inventory.IsFull) break;

                    restActivity.Inventory.RemoveItem(item, 1);
                    unit.Inventory.AddItem(item);
                }

                if (unit.Inventory.IsFull) break;
            }
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }

    public class RestInventoryDisplayItem : CardDrawerDisplayItem
    {
        public RestInventoryDisplayItem(RestActivity restActivity, System.Action onChanged)
        {
            condition = () => true;
            color = new Color(0.95f, 0.95f, 1f);
            drawAction = () =>
            {
                EditorGUILayout.Space(4);

                if (restActivity?.Inventory != null)
                {
                    var items = restActivity.Inventory.ItemStacks;

                    if (items.Any())
                    {
                        EditorGUILayout.LabelField("Storage Inventory:", EditorStyles.boldLabel);
                        EditorGUILayout.Space(2);

                        foreach (var itemStack in items)
                        {
                            var item = itemStack.item;
                            var count = itemStack.count;

                            EditorGUILayout.BeginHorizontal();

                            // Item icon
                            if (item?.Sprite?.texture != null)
                            {
                                var iconRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20), GUILayout.Height(20));
                                GUI.DrawTexture(iconRect, item.Sprite.texture, ScaleMode.ScaleToFit);
                            }
                            else
                            {
                                GUILayout.Space(20);
                            }

                            // Item name and count
                            EditorGUILayout.LabelField($"{item?.DisplayName ?? "Unknown"} x{count}", GUILayout.Width(150));

                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space(2);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Storage is empty", EditorStyles.miniLabel);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No storage inventory found", EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(4);
            };
        }
    }
}
#endif
