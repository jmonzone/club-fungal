#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class PartyUnitCardDrawer
    {
        private static Dictionary<string, CollectionState> _collectionStates = new Dictionary<string, CollectionState>();

        private class CollectionState
        {
            public float LastProgress;
            public double LastCollectionTime;
            public int LastLevel;
            public double LastLevelUpTime;
        }

        // Simple draw for party units (no activity)
        public void Draw(UnitInstance unit)
        {
            DrawCard(unit, null, null, null, null, null, null, null);
        }

        // Draw with custom display items (for unlock activities, etc.)
        public void Draw(UnitInstance unit, System.Action extraDisplayItems)
        {
            DrawCard(unit, null, null, null, null, extraDisplayItems, null, null);
        }

        // Draw with custom progress and status (for unlock/other activities)
        public void Draw(
            UnitInstance unit,
            System.Action extraDisplayItems,
            System.Func<float> progressProvider,
            System.Action statusDisplay)
        {
            DrawCard(unit, null, null, null, null, extraDisplayItems, progressProvider, statusDisplay);
        }

        // Full draw for resource activity units
        public void Draw(
            UnitInstance unit,
            ActivityInstance activity,
            ResourceUpdateComponent resourceComponent,
            NetworkRun currentRun,
            System.Action onChanged)
        {
            DrawCard(unit, activity, resourceComponent, currentRun, onChanged, null, null, null);
        }

        private void DrawCard(
            UnitInstance unit,
            ActivityInstance activity,
            ResourceUpdateComponent resourceComponent,
            NetworkRun currentRun,
            System.Action onChanged,
            System.Action extraDisplayItems,
            System.Func<float> progressProvider,
            System.Action statusDisplay)
        {
            var hasActivityInfo = activity != null && resourceComponent != null;
            var hasExtraDisplayItems = extraDisplayItems != null;
            var hasCustomProgress = progressProvider != null || statusDisplay != null;
            var isInActivity = hasActivityInfo && activity.Units != null && activity.Units.Contains(unit);
            var height = hasActivityInfo ? 125 : ((hasExtraDisplayItems || hasCustomProgress) ? 125 : 80);

            var cardRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(100), GUILayout.Height(height), GUILayout.ExpandWidth(false));

            // Draw drag handle at top
            DrawDragHandle(unit);

            // Block drop zone from accepting drops on this card
            var evt = Event.current;
            if (cardRect.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    evt.Use();
                }
                else if (evt.type == EventType.DragPerform)
                {
                    evt.Use();
                }
            }

            // Always draw: icon, name
            DrawIcon(unit);
            DrawName(unit);
            DrawSpeciesType(unit);

            DrawUnitInventory(unit);
            DrawMaxInventory(unit);

            // Draw extra display items if provided
            if (hasExtraDisplayItems)
            {
                extraDisplayItems?.Invoke();
            }

            // Draw custom progress/status if provided
            if (hasCustomProgress && !hasActivityInfo)
            {
                if (progressProvider != null)
                {
                    var progress = progressProvider.Invoke();
                    DrawProgressBar(progress);
                }
                if (statusDisplay != null)
                {
                    statusDisplay.Invoke();
                }
            }

            // Draw activity-specific info or just species type
            if (hasActivityInfo)
            {
                DrawDivider();

                DrawSkillLevel(unit, activity);
                DrawXPProgressBar(unit, activity);

                if (currentRun.Settings.debugMode)
                {
                    DrawSpeedBonus(unit, activity, resourceComponent);
                }
                // DrawDivider();

                if (currentRun.Settings.debugMode)
                {
                    DrawResourceCount(unit, resourceComponent);
                }

                if (currentRun.Settings.debugMode)
                {
                    DrawClaimButton(unit, resourceComponent, currentRun, onChanged);
                }

                DrawDivider();

                if (isInActivity)
                {
                    DrawActivityStatus(unit, activity, resourceComponent, currentRun);
                    if (currentRun.Settings.debugMode)
                    {
                        DrawStopButton(unit, activity, onChanged);
                    }
                }
                else
                {
                    GUILayout.Space(10);
                    DrawStartButton(unit, activity, currentRun, onChanged);
                }

                if (currentRun.Settings.debugMode)
                {
                    DrawViewDataButton(unit);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawDragHandle(UnitInstance unit)
        {
            var evt = Event.current;

            // Create drag handle area at top of card
            var handleRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(14), GUILayout.Width(90));

            GUILayout.FlexibleSpace();

            var handleStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
            };

            EditorGUILayout.LabelField("⋮⋮", handleStyle, GUILayout.Width(20), GUILayout.Height(14));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            // Get actual rect after layout
            var actualRect = GUILayoutUtility.GetLastRect();

            // Enable drag from handle only
            if (evt.type == EventType.MouseDown && actualRect.Contains(evt.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new UnityEngine.Object[] { };
                DragAndDrop.SetGenericData("UnitInstance", unit);
                DragAndDrop.StartDrag(unit.DisplayName);
                evt.Use();
            }

            // Change cursor when hovering over handle
            if (actualRect.Contains(evt.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(actualRect, MouseCursor.MoveArrow);
            }
        }

        private void DrawIcon(UnitInstance unit)
        {
            var icon = unit.Species?.Sprite?.texture;
            if (icon != null)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
                GUILayout.FlexibleSpace();
                GUILayout.Box(icon, GUILayout.Width(40), GUILayout.Height(40));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(40);
            }
        }

        private void DrawName(UnitInstance unit)
        {
            var nameStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            EditorGUILayout.LabelField(unit.DisplayName, nameStyle, GUILayout.Height(16), GUILayout.Width(90));
        }

        private void DrawSpeciesType(UnitInstance unit)
        {
            if (unit.Species?.Type != null)
            {
                var typeStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 8,
                    normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
                };
                EditorGUILayout.LabelField(unit.Species.Type.Id, typeStyle, GUILayout.Height(12), GUILayout.Width(90));
            }
        }

        private void DrawSkillLevel(UnitInstance unit, ActivityInstance activity)
        {
            if (activity?.Template?.PrimarySkill != null && unit.Skills != null)
            {
                if (unit.Skills.TryGetValue(activity.Template.PrimarySkill, out var skillInstance))
                {
                    var skillText = $"{activity.Template.PrimarySkill.Id} Lv.{skillInstance.Level}";
                    var skillStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 8,
                        normal = { textColor = new Color(0.7f, 0.9f, 1f) }
                    };
                    EditorGUILayout.LabelField(skillText, skillStyle, GUILayout.Height(10), GUILayout.Width(90));
                }
                else
                {
                    GUILayout.Space(10);
                }
            }
            else
            {
                GUILayout.Space(10);
            }
        }

        private void DrawXPProgressBar(UnitInstance unit, ActivityInstance activity)
        {
            if (activity?.Template?.PrimarySkill != null && unit.Skills != null)
            {
                if (unit.Skills.TryGetValue(activity.Template.PrimarySkill, out var skillInstance))
                {
                    var currentLevel = skillInstance.Level;
                    var currentXP = skillInstance.XP;
                    var xpForCurrentLevel = SkillInstance.GetXPFromLevel(currentLevel);
                    var xpForNextLevel = SkillInstance.GetXPFromLevel(currentLevel + 1);
                    var xpIntoLevel = currentXP - xpForCurrentLevel;
                    var xpNeededForLevel = xpForNextLevel - xpForCurrentLevel;
                    var progress = xpIntoLevel / xpNeededForLevel;

                    var progressRect = EditorGUILayout.GetControlRect(false, 4, GUILayout.Width(90));
                    EditorGUI.ProgressBar(progressRect, progress, "");
                }
                else
                {
                    GUILayout.Space(4);
                }
            }
            else
            {
                GUILayout.Space(4);
            }
        }

        private void DrawResourceCount(UnitInstance unit, ResourceUpdateComponent resourceComponent)
        {
            var item = resourceComponent.ItemTemplate;
            var count = unit.Inventory.GetItemCount(item);
            DrawResourceItem(item, count);
        }

        private void DrawResourceItem(ItemTemplate item, int? count = null, string labelOverride = null)
        {
            if (item == null) { GUILayout.Space(10); return; }
            var label = labelOverride ?? (count.HasValue ? $"{count.Value}x {item.DisplayName ?? item.Id}" : item.DisplayName ?? item.Id);
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 8,
                normal = { textColor = count.HasValue && count.Value > 0 ? new Color(0.5f, 1f, 0.5f) : new Color(0.7f, 0.7f, 0.7f) }
            };
            EditorGUILayout.LabelField(label, style, GUILayout.Height(14), GUILayout.Width(90));
        }

        private void DrawInventoryItem(ItemTemplate item, int count)
        {
            if (item == null) return;
            var icon = item.Sprite != null ? item.Sprite.texture : null;
            var label = $"{count}x {item.DisplayName ?? item.Id}";
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 8,
                normal = { textColor = count > 0 ? new Color(0.5f, 1f, 0.5f) : new Color(0.7f, 0.7f, 0.7f) }
            };
            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            if (icon != null)
                GUILayout.Box(icon, GUILayout.Width(14), GUILayout.Height(14));
            else
                GUILayout.Space(14);
            EditorGUILayout.LabelField(label, style, GUILayout.Height(14), GUILayout.Width(66));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawUnitInventory(UnitInstance unit)
        {
            if (unit?.Inventory?.ItemStacks != null && unit.Inventory.ItemStacks.Count > 0)
            {
                foreach (var itemStack in unit.Inventory.ItemStacks)
                {
                    if (itemStack?.item != null && itemStack.count > 0)
                    {
                        DrawInventoryItem(itemStack.item, itemStack.count);
                    }
                }
            }
            else
            {
                var emptyStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 8,
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
                };
                EditorGUILayout.LabelField("empty inventory", emptyStyle, GUILayout.Height(14), GUILayout.Width(90));
            }
        }

        private void DrawMaxInventory(UnitInstance unit)
        {
            if (unit?.Inventory != null)
            {
                var maxCapacity = unit.Inventory.MaxCapacity;
                var totalCount = unit.Inventory.TotalItemCount;
                var capacityText = maxCapacity > 0 ? $"Capacity: {totalCount}/{maxCapacity}" : $"Capacity: {totalCount}/∞";
                var capacityStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 7,
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
                };
                EditorGUILayout.LabelField(capacityText, capacityStyle, GUILayout.Height(10), GUILayout.Width(90));
            }
            else
            {
                GUILayout.Space(10);
            }
        }

        private void DrawDivider()
        {
            GUILayout.Space(2);
            var rect = EditorGUILayout.GetControlRect(false, 1, GUILayout.Width(90));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            GUILayout.Space(2);
        }

        private void DrawClaimButton(UnitInstance unit, ResourceUpdateComponent resourceComponent, NetworkRun currentRun, System.Action onChanged)
        {
            var count = unit.Inventory.GetItemCount(resourceComponent.ItemTemplate);
            if (count > 0)
            {
                GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
                if (GUILayout.Button("Claim", GUILayout.Height(16), GUILayout.Width(90)))
                {
                    unit.ClaimItemsToInventory(resourceComponent.ItemTemplate, currentRun.Inventory);
                    onChanged?.Invoke();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button("Claim", GUILayout.Height(16), GUILayout.Width(90));
                GUI.enabled = true;
            }
        }

        private void DrawSpeedBonus(UnitInstance unit, ActivityInstance activity, ResourceUpdateComponent resourceComponent)
        {
            var speedBonus = resourceComponent.GetSpeedBonus(unit, activity);
            if (speedBonus > 1f)
            {
                var bonusText = $"Speed: {speedBonus:F1}x";
                var bonusStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 8,
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = new Color(1f, 0.85f, 0.3f) }
                };
                EditorGUILayout.LabelField(bonusText, bonusStyle, GUILayout.Height(10), GUILayout.Width(90));
            }
            else
            {
                GUILayout.Space(10);
            }
        }

        private void DrawActivityStatus(UnitInstance unit, ActivityInstance activity, ResourceUpdateComponent resourceComponent, NetworkRun currentRun)
        {
            var progress = resourceComponent.GetUnitProgress(unit, activity, currentRun);

            // Track collection state for this unit
            var stateKey = $"{activity.GetHashCode()}_{unit.GetHashCode()}";
            if (!_collectionStates.ContainsKey(stateKey))
            {
                _collectionStates[stateKey] = new CollectionState { LastProgress = progress };
            }
            var state = _collectionStates[stateKey];

            // Detect collection completion (progress went from high to low)
            if (state.LastProgress > 0.8f && progress < 0.2f)
            {
                state.LastCollectionTime = EditorApplication.timeSinceStartup;
            }
            state.LastProgress = progress;

            // Detect level up
            if (activity?.Template?.PrimarySkill != null && unit?.Skills != null)
            {
                if (unit.Skills.TryGetValue(activity.Template.PrimarySkill, out var skillInstance))
                {
                    var currentLevel = skillInstance.Level;
                    if (state.LastLevel > 0 && currentLevel > state.LastLevel)
                    {
                        state.LastLevelUpTime = EditorApplication.timeSinceStartup;
                    }
                    state.LastLevel = currentLevel;
                }
            }

            // Only draw progress bar if inventory is not full
            if (unit?.Inventory == null || !unit.Inventory.IsFull)
            {
                DrawProgressBar(progress);
            }
            DrawStatusText(state, progress, resourceComponent, unit, activity);
        }

        private void DrawStatusText(CollectionState state, float progress, ResourceUpdateComponent resourceComponent, UnitInstance unit, ActivityInstance activity)
        {
            var timeSinceLevelUp = EditorApplication.timeSinceStartup - state.LastLevelUpTime;
            var timeSinceCollection = EditorApplication.timeSinceStartup - state.LastCollectionTime;
            var item = resourceComponent.ItemTemplate;
            string statusText = null;
            Color statusColor = Color.white;
            string labelOverride = null;

            // Priority 0: Level up message (show for 2 seconds)
            if (timeSinceLevelUp < 2.0)
            {
                var currentLevel = 1;
                if (activity?.Template?.PrimarySkill != null && unit?.Skills != null)
                {
                    if (unit.Skills.TryGetValue(activity.Template.PrimarySkill, out var skillInstance))
                    {
                        currentLevel = skillInstance.Level;
                    }
                }
                statusText = $"Level Up! Lv.{currentLevel}";
                statusColor = new Color(1f, 0.8f, 0f);
            }
            // Priority 1: Inventory full (show when inventory is full)
            else if (unit?.Inventory != null && unit.Inventory.IsFull)
            {
                statusText = "INVENTORY FULL";
                statusColor = new Color(1f, 0.5f, 0.5f);
            }
            // Priority 2: Collection message (show for 2 seconds)
            else if (timeSinceCollection < 2.0)
            {
                statusText = $"+{resourceComponent.ItemsPerUpdate}";
                statusColor = new Color(0.3f, 1f, 0.3f);
                labelOverride = statusText + (item != null ? $" {item.DisplayName ?? item.Id}" : "");
            }
            // Priority 3: Collecting status
            else if (progress > 0)
            {
                statusText = "Collecting...";
                statusColor = new Color(0.7f, 0.7f, 1f);
                labelOverride = statusText;
            }

            if (!string.IsNullOrEmpty(labelOverride) && item != null)
            {
                DrawResourceItem(item, null, labelOverride);
            }
            else if (!string.IsNullOrEmpty(statusText))
            {
                var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = statusColor },
                    fontSize = 9
                };
                EditorGUILayout.LabelField(statusText, statusStyle, GUILayout.Height(12), GUILayout.Width(90));
            }
            else
            {
                GUILayout.Space(12);
            }
        }

        private void DrawProgressBar(float progress)
        {
            var unitProgressRect = EditorGUILayout.GetControlRect(false, 8, GUILayout.Width(90));
            EditorGUI.ProgressBar(unitProgressRect, progress, "");
        }

        private void DrawStopButton(UnitInstance unit, ActivityInstance activity, System.Action onChanged)
        {
            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
            if (GUILayout.Button("Stop", GUILayout.Height(18), GUILayout.Width(90)))
            {
                activity.RemoveUnit(unit);
                UnityEditor.AssetDatabase.SaveAssets();
                onChanged?.Invoke();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawStartButton(UnitInstance unit, ActivityInstance activity, NetworkRun currentRun, System.Action onChanged)
        {
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
            if (GUILayout.Button("Start", GUILayout.Height(18), GUILayout.Width(90)))
            {
                var allActivities = currentRun?.CurrentRoom?.Data?.activities;
                activity.AddUnit(unit, allActivities);
                UnityEditor.AssetDatabase.SaveAssets();
                onChanged?.Invoke();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawViewDataButton(UnitInstance unit)
        {
            GUI.backgroundColor = new Color(0.85f, 0.85f, 1f);
            if (GUILayout.Button("View Data", GUILayout.Height(16), GUILayout.Width(90)))
            {
                UnitInstanceInspectorWindow.ShowWindow(unit);
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
