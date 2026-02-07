#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ResourceUnitCardDrawer
    {
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
            var isInActivity = activity.Units != null && activity.Units.Contains(unit);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(100), GUILayout.Height(125), GUILayout.ExpandWidth(false));

            DrawIcon(unit);
            DrawName(unit);
            DrawSkillLevel(unit, activity);
            DrawXPProgressBar(unit, activity);
            DrawSpeedBonus(unit, activity, resourceComponent);
            DrawDivider();
            DrawResourceCount(unit, resourceComponent);
            DrawClaimButton(unit, resourceComponent, currentRun, onChanged);

            DrawDivider();

            if (isInActivity)
            {
                DrawActivityStatus(unit, activity, resourceComponent);
                DrawStopButton(unit, activity, onChanged);
            }
            else
            {
                GUILayout.Space(10);
                DrawStartButton(unit, activity, currentRun, onChanged);
            }

            DrawViewDataButton(unit);

            EditorGUILayout.EndVertical();
        }

        private void DrawIcon(UnitInstance unit)
        {
            var icon = unit.Species?.Sprite?.texture;
            if (icon != null)
            {
                EditorGUILayout.BeginHorizontal();
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
            EditorGUILayout.LabelField(unit.DisplayName, nameStyle, GUILayout.Height(16));
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
            if (resourceComponent?.ItemTemplate != null)
            {
                var count = unit.Inventory.GetItemCount(resourceComponent.ItemTemplate);
                var resourceName = resourceComponent.ItemTemplate.DisplayName ?? resourceComponent.ItemTemplate.Id;
                string countText;
                Color textColor;
                if (count > 0)
                {
                    countText = $"{count}x {resourceName}";
                    textColor = new Color(0.5f, 1f, 0.5f);
                }
                else
                {
                    countText = "No resources";
                    textColor = new Color(0.7f, 0.7f, 0.7f);
                }
                var countStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 8,
                    normal = { textColor = textColor }
                };
                EditorGUILayout.LabelField(countText, countStyle, GUILayout.Height(10), GUILayout.Width(90));
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
            DrawButtonOrEmpty(
                show: resourceComponent?.ItemTemplate != null,
                buttonLabel: () =>
                {
                    var count = unit.Inventory.GetItemCount(resourceComponent.ItemTemplate);
                    return count > 0 ? $"Claim" : null;
                },
                onClick: () =>
                {
                    unit.ClaimItemsToInventory(resourceComponent.ItemTemplate, currentRun.Inventory);
                    onChanged?.Invoke();
                },
                height: 16
            );
        }

        // DRY helper for button or empty state
        private void DrawButtonOrEmpty(bool show, System.Func<string> buttonLabel, System.Action onClick, int height)
        {
            if (!show)
            {
                GUILayout.Space(height);
                return;
            }
            var label = buttonLabel();
            if (!string.IsNullOrEmpty(label))
            {
                GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
                if (GUILayout.Button(label, GUILayout.Height(height), GUILayout.Width(90)))
                {
                    onClick?.Invoke();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button("Claim", GUILayout.Height(height), GUILayout.Width(90));
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

        private void DrawActivityStatus(UnitInstance unit, ActivityInstance activity, ResourceUpdateComponent resourceComponent)
        {
            var progress = resourceComponent.GetUnitProgress(unit, activity);

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

            DrawStatusText(state, progress, resourceComponent, unit, activity);
            DrawProgressBar(progress);
        }

        private void DrawStatusText(CollectionState state, float progress, ResourceUpdateComponent resourceComponent, UnitInstance unit, ActivityInstance activity)
        {
            var timeSinceLevelUp = EditorApplication.timeSinceStartup - state.LastLevelUpTime;
            var timeSinceCollection = EditorApplication.timeSinceStartup - state.LastCollectionTime;
            var itemName = resourceComponent.ItemTemplate?.DisplayName ?? "Unknown";
            string statusText;
            Color statusColor;

            // Priority 1: Level up message (show for 2 seconds)
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
            // Priority 2: Collection message (show for 2 seconds)
            else if (timeSinceCollection < 2.0)
            {
                statusText = $"+{resourceComponent.ItemsPerUpdate} {itemName}";
                statusColor = new Color(0.3f, 1f, 0.3f);
            }
            // Priority 3: Collecting status
            else if (progress > 0)
            {
                statusText = "Collecting...";
                statusColor = new Color(0.7f, 0.7f, 1f);
            }
            else
            {
                statusText = "";
                statusColor = Color.white;
            }

            if (!string.IsNullOrEmpty(statusText))
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
