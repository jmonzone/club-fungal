#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ResourceUnitCardDrawer
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();
        private static Dictionary<string, CollectionState> _collectionStates = new Dictionary<string, CollectionState>();

        private class CollectionState
        {
            public float LastProgress;
            public double LastCollectionTime;
            public int LastLevel;
            public double LastLevelUpTime;
        }

        public void Draw(
            UnitInstance unit,
            ActivityInstance activity,
            ResourceUpdateComponent resourceComponent,
            NetworkRun currentRun,
            System.Action onChanged)
        {
            var isInActivity = activity?.Units != null && activity.Units.Contains(unit);
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

            _cardDrawer.Draw(
                unit,
                () =>
                {
                    DrawDivider();

                    // Extra display items (skill, xp, buttons)
                    DrawSkillLevel(unit, activity);
                    DrawXPProgressBar(unit, activity);

                    if (currentRun.Settings.debugMode)
                    {
                        DrawSpeedBonus(unit, activity, resourceComponent);
                    }

                    if (currentRun.Settings.debugMode)
                    {
                        DrawResourceCount(unit, resourceComponent);
                    }

                    if (currentRun.Settings.debugMode)
                    {
                        DrawClaimButton(unit, resourceComponent, currentRun, onChanged);
                    }

                    DrawDivider();

                    // Stop/Start buttons
                    if (isInActivity)
                    {
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
                },
                (isInActivity && (unit?.Inventory == null || !unit.Inventory.IsFull)) ? () => progress : null,
                () =>
                {
                    if (isInActivity)
                    {
                        DrawStatusText(state, progress, resourceComponent, unit, activity);
                    }
                }
            );
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
                    var progressValue = xpIntoLevel / xpNeededForLevel;

                    var progressRect = EditorGUILayout.GetControlRect(false, 4, GUILayout.Width(90));
                    EditorGUI.ProgressBar(progressRect, progressValue, "");
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

        private void DrawDivider()
        {
            GUILayout.Space(2);
            var rect = EditorGUILayout.GetControlRect(false, 1, GUILayout.Width(90));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            GUILayout.Space(2);
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
    }
}
#endif
