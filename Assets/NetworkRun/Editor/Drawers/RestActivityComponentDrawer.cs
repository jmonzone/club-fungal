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
                    // Show energy restoration
                    DrawEnergyProgressBar(unit, currentRun);

                    // View Unit button
                    if (currentRun.Settings.showViewUnitButton)
                    {
                        GUI.backgroundColor = new Color(0.85f, 0.85f, 1f);
                        if (GUILayout.Button("View Unit", GUILayout.Height(18), GUILayout.Width(90)))
                        {
                            UnitUpgradeWindow.ShowWindow(unit, currentRun);
                        }
                        GUI.backgroundColor = Color.white;
                    }

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
                    // Show transfer buttons (debug mode)
                    if (component?.Inventory != null && unit?.Inventory != null && currentRun.Settings.debugMode)
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

                    // Eat Fish button
                    if (component?.Inventory != null && unit?.Inventory != null)
                    {
                        var fishItem = currentRun?.Settings?.fishItem;
                        if (fishItem != null)
                        {
                            var unitFishCount = unit.Inventory.GetItemCount(fishItem);
                            var storageFishCount = component.Inventory.GetItemCount(fishItem);

                            // Check if any other unit in the activity has fish
                            var otherUnitHasFish = false;
                            if (activity?.Units != null)
                            {
                                foreach (var otherUnit in activity.Units)
                                {
                                    if (otherUnit != null && otherUnit != unit && otherUnit.Inventory.GetItemCount(fishItem) > 0)
                                    {
                                        otherUnitHasFish = true;
                                        break;
                                    }
                                }
                            }

                            var hasFish = unitFishCount > 0 || storageFishCount > 0 || otherUnitHasFish;

                            // Only show fish eating option if energy is depleted
                            if (hasFish && unit.Energy < 100f)
                            {
                                GUI.backgroundColor = new Color(1f, 0.9f, 0.6f);
                                if (GUILayout.Button("🍴 Eat Fish", GUILayout.Height(20), GUILayout.Width(90)))
                                {
                                    EatFish(unit, component, fishItem, activity);
                                    onChanged?.Invoke();
                                }
                                GUI.backgroundColor = Color.white;
                            }
                            else if (unit.Energy < 100f)
                            {
                                GUI.enabled = false;
                                GUILayout.Button("No Food", GUILayout.Height(20), GUILayout.Width(90));
                                GUI.enabled = true;
                            }

                            EditorGUILayout.Space(2);
                        }
                    }
                },
                currentRun?.Settings);
        }

        private void DrawEnergyProgressBar(UnitInstance unit, NetworkRun currentRun)
        {
            if (unit != null)
            {
                EditorGUILayout.Space(2);

                if (unit.Energy >= 100f)
                {
                    // Show "ready to go!" when fully rested
                    var readyStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 9,
                        fontStyle = FontStyle.Bold,
                        normal = { textColor = new Color(0.3f, 1f, 0.3f) }
                    };
                    EditorGUILayout.LabelField("✓ Ready to go!", readyStyle, GUILayout.Height(18), GUILayout.Width(90));
                }
                else
                {
                    if (currentRun.Settings.debugMode)
                    {
                        // Show progress bar while resting
                        var energyValue = unit.Energy / 100f;
                        var progressRect = EditorGUILayout.GetControlRect(false, 8, GUILayout.Width(90));

                        // Color based on energy level (green tones for restoration)
                        var originalColor = GUI.color;
                        if (energyValue > 0.5f)
                            GUI.color = new Color(0.3f, 1f, 0.3f); // Green
                        else if (energyValue > 0.25f)
                            GUI.color = new Color(1f, 0.8f, 0.3f); // Yellow
                        else
                            GUI.color = new Color(1f, 0.3f, 0.3f); // Red

                        EditorGUI.ProgressBar(progressRect, energyValue, $"Energy: {unit.Energy:F0}%");
                        GUI.color = originalColor;
                    }


                    // Show resting status
                    var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 8,
                        normal = { textColor = new Color(0.5f, 1f, 0.5f) }
                    };
                    EditorGUILayout.LabelField("💤 Resting...", statusStyle, GUILayout.Height(10), GUILayout.Width(90));
                }

                EditorGUILayout.Space(2);
            }
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

        private void EatFish(UnitInstance unit, RestActivity restActivity, ItemTemplate fishItem, ActivityInstance activity)
        {
            // First try to eat from unit inventory
            if (unit.Inventory.GetItemCount(fishItem) > 0)
            {
                unit.Inventory.RemoveItem(fishItem, 1);
                Debug.Log($"{unit.DisplayName} ate fish from their inventory");
            }
            // Otherwise eat from storage
            else if (restActivity.Inventory.GetItemCount(fishItem) > 0)
            {
                restActivity.Inventory.RemoveItem(fishItem, 1);
                Debug.Log($"{unit.DisplayName} ate fish from storage");
            }
            // Otherwise check other units in the activity
            else if (activity?.Units != null)
            {
                foreach (var otherUnit in activity.Units)
                {
                    if (otherUnit != null && otherUnit != unit && otherUnit.Inventory.GetItemCount(fishItem) > 0)
                    {
                        otherUnit.Inventory.RemoveItem(fishItem, 1);
                        Debug.Log($"{unit.DisplayName} ate fish from {otherUnit.DisplayName}'s inventory");
                        break;
                    }
                }
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
                        // EditorGUILayout.LabelField("Storage is empty", EditorStyles.miniLabel);
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
