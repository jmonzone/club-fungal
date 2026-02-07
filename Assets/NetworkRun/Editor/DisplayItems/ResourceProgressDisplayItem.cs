#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ResourceProgressDisplayItem : UnitDrawerDisplayItem
    {
        private static Dictionary<string, CollectionState> _collectionStates = new Dictionary<string, CollectionState>();

        private class CollectionState
        {
            public float LastProgress;
            public double LastCollectionTime;
        }

        public ResourceProgressDisplayItem(ResourceUpdateComponent resourceComponent, ActivityInstance activity, NetworkRun currentRun, System.Action onChanged)
        {
            condition = () => true;
            color = Color.white;
            drawAction = () =>
            {
                EditorGUILayout.Space(4);

                var unitCount = activity.Units?.Count ?? 0;
                var itemName = resourceComponent.ItemTemplate?.DisplayName ?? "Unknown";
                var progress = unitCount > 0 ? resourceComponent.Progress : 0f;

                // Show all party units in a grid
                if (currentRun?.Party != null && currentRun.Party.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    int count = 0;
                    const int itemsPerRow = 3;

                    foreach (var unit in currentRun.Party)
                    {
                        if (unit == null) continue;

                        if (count > 0 && count % itemsPerRow == 0)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space(2);
                            EditorGUILayout.BeginHorizontal();
                        }

                        var isInActivity = activity.Units != null && activity.Units.Contains(unit);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(100), GUILayout.Height(115), GUILayout.ExpandWidth(false));

                        // Unit icon (centered)
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

                        // Unit name
                        var nameStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
                        EditorGUILayout.LabelField(unit.DisplayName, nameStyle, GUILayout.Height(16));

                        if (isInActivity)
                        {
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

                            // Show status text
                            var timeSinceCollection = EditorApplication.timeSinceStartup - state.LastCollectionTime;
                            string statusText;
                            Color statusColor;

                            if (timeSinceCollection < 2.0)
                            {
                                statusText = $"+{resourceComponent.ItemsPerUpdate} {itemName}";
                                statusColor = new Color(0.3f, 1f, 0.3f);
                            }
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

                        if (isInActivity)
                        {
                            // Progress indicator
                            var unitProgress = progress;
                            var unitProgressRect = EditorGUILayout.GetControlRect(false, 8, GUILayout.Width(90));
                            EditorGUI.ProgressBar(unitProgressRect, unitProgress, "");

                            // Remove button
                            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                            if (GUILayout.Button("Remove", GUILayout.Height(18), GUILayout.Width(90)))
                            {
                                activity.RemoveUnit(unit);
                                UnityEditor.AssetDatabase.SaveAssets();
                                onChanged?.Invoke();
                            }
                            GUI.backgroundColor = Color.white;
                        }
                        else
                        {
                            GUILayout.Space(20);

                            // Add button
                            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                            if (GUILayout.Button("Add", GUILayout.Height(18), GUILayout.Width(90)))
                            {
                                var allActivities = currentRun?.CurrentRoom?.Data?.activities;
                                activity.AddUnit(unit, allActivities);
                                UnityEditor.AssetDatabase.SaveAssets();
                                onChanged?.Invoke();
                            }
                            GUI.backgroundColor = Color.white;
                        }

                        EditorGUILayout.EndVertical();

                        count++;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);

                    // Show what's being collected
                    var totalItems = resourceComponent.ItemsPerUpdate * unitCount;
                    EditorGUILayout.LabelField($"⏱ Each cycle: {totalItems}x {itemName} ({resourceComponent.UpdateInterval:F1}s)", EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(2);
            };
        }
    }
}
#endif
