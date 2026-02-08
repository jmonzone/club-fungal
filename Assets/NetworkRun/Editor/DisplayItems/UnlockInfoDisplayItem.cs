#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class UnlockInfoDisplayItem : UnitDrawerDisplayItem
    {
        private UnitDropZoneDrawer _dropZoneDrawer = new UnitDropZoneDrawer();
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();

        public UnlockInfoDisplayItem(ActivityInstance activity, NetworkRun currentRun, UnlockComponent unlockComponent, System.Action onChanged)
        {
            condition = () => true;
            color = Color.white;
            drawAction = () =>
            {
                EditorGUILayout.Space(4);

                // Show door conditions using the assigned door from the component
                var door = unlockComponent.AssignedDoor;
                if (door != null && unlockComponent.ResourceCondition != null)
                {
                    var hasEnough = unlockComponent.CurrentResourceCount >= unlockComponent.RequiredAmount;
                    var resourceName = unlockComponent.ResourceCondition.RequiredItem?.DisplayName ?? "Unknown";

                    // Show resource progress
                    EditorGUILayout.BeginHorizontal();

                    // Draw resource icon
                    var resourceItem = unlockComponent.ResourceCondition.RequiredItem;
                    if (resourceItem?.Sprite != null)
                    {
                        var icon = resourceItem.Sprite.texture;
                        GUILayout.Box(icon, GUILayout.Width(20), GUILayout.Height(20));
                    }
                    else
                    {
                        GUILayout.Space(20);
                    }

                    // Show progress bar with resource name
                    var progress = Mathf.Clamp01((float)unlockComponent.CurrentResourceCount / unlockComponent.RequiredAmount);
                    var rect = EditorGUILayout.GetControlRect(false, 20);
                    EditorGUI.ProgressBar(rect, progress, $"{resourceName}: {unlockComponent.CurrentResourceCount}/{unlockComponent.RequiredAmount}");

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(4);

                    // Draw unit drop zone
                    DrawUnitsDropZone(activity, currentRun, unlockComponent, resourceName, onChanged);

                    // Show Open Door button if enough resources collected
                    if (hasEnough)
                    {
                        EditorGUILayout.Space(4);
                        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                        var buttonText = $"🚪 Open Door";
                        if (GUILayout.Button(buttonText, GUILayout.Height(30)))
                        {
                            unlockComponent.CompleteTask(currentRun);
                            currentRun.OpenDoorAndTransition(door);
                            onChanged?.Invoke();
                        }
                        GUI.backgroundColor = Color.white;
                    }

                    EditorGUILayout.Space(2);
                }
            };
        }

        private void DrawUnitsDropZone(ActivityInstance activity, NetworkRun currentRun, UnlockComponent unlockComponent, string resourceName, System.Action onChanged)
        {
            _dropZoneDrawer.Draw(
                "🖱️ Drop units here to assign to unlock task",
                (contentRect) =>
                {
                    if (activity.Units != null && activity.Units.Count > 0)
                    {
                        var activeHeaderStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontStyle = FontStyle.Bold,
                            normal = { textColor = new Color(0.5f, 1f, 0.5f) }
                        };
                        EditorGUILayout.LabelField($"Active Units ({activity.Units.Count})", activeHeaderStyle, GUILayout.Height(14));
                        EditorGUILayout.Space(2);

                        EditorGUILayout.BeginHorizontal();
                        int count = 0;
                        const int itemsPerRow = 3;

                        foreach (var unit in activity.Units)
                        {
                            if (unit == null) continue;

                            if (count > 0 && count % itemsPerRow == 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space(2);
                                EditorGUILayout.BeginHorizontal();
                            }

                            var resourceItem = unlockComponent.ResourceCondition?.RequiredItem;
                            var hasResource = resourceItem != null && unit.Inventory.GetItemCount(resourceItem) > 0;
                            var isUnlocked = unlockComponent.IsUnlocked;

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
                                (hasResource && !isUnlocked) ? () => unlockComponent.GetUnitProgress(unit) / unlockComponent.UpdateInterval : null,
                                () =>
                                {
                                    // Status with resource info
                                    if (isUnlocked)
                                    {
                                        var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                                        {
                                            alignment = TextAnchor.MiddleCenter,
                                            fontSize = 8,
                                            normal = { textColor = new Color(1f, 0.85f, 0.3f) }
                                        };
                                        EditorGUILayout.LabelField("✓ Task Complete", statusStyle, GUILayout.Height(12), GUILayout.Width(90));
                                    }
                                    else if (hasResource)
                                    {
                                        var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                                        {
                                            alignment = TextAnchor.MiddleCenter,
                                            fontSize = 8,
                                            normal = { textColor = new Color(0.7f, 1f, 0.7f) }
                                        };
                                        EditorGUILayout.LabelField("Contributing...", statusStyle, GUILayout.Height(12), GUILayout.Width(90));
                                    }
                                    else
                                    {
                                        var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                                        {
                                            alignment = TextAnchor.MiddleCenter,
                                            fontSize = 8,
                                            normal = { textColor = new Color(1f, 0.5f, 0.5f) }
                                        };
                                        var resourceName = resourceItem?.DisplayName ?? "resource";
                                        EditorGUILayout.LabelField($"Needs {resourceName}", statusStyle, GUILayout.Height(12), GUILayout.Width(90));
                                    }

                                    GUILayout.Space(2);
                                });
                            count++;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(40);
                        var emptyStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 10,
                            normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
                        };
                        EditorGUILayout.LabelField($"⏱ No units assigned. Drag units here with {resourceName} in inventory.", emptyStyle);
                        GUILayout.Space(40);
                    }
                },
                (draggedUnit) => !(activity.Units != null && activity.Units.Contains(draggedUnit)),
                (draggedUnit) =>
                {
                    var allActivities = currentRun?.CurrentRoom?.Data?.activities;
                    activity.AddUnit(draggedUnit, allActivities);
                    UnityEditor.AssetDatabase.SaveAssets();
                    onChanged?.Invoke();
                },
                DragAndDropVisualMode.Copy
            );
        }


    }
}
#endif
