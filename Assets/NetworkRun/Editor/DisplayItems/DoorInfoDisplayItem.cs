#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class DoorInfoDisplayItem : CardDrawerDisplayItem
    {
        public DoorInfoDisplayItem(NetworkRun currentRun, InspectComponent inspectComponent, System.Action onChanged, ActivityInstance activityInstance)
        {
            condition = () => true;
            color = Color.white;
            drawAction = () =>
            {
                EditorGUILayout.Space(4);

                // Show door conditions using the assigned door from the component
                var door = inspectComponent.AssignedDoor;
                if (door != null)
                {
                    if (door.conditions != null && door.conditions.Count > 0)
                    {
                        foreach (var doorCondition in door.conditions)
                        {
                            if (doorCondition != null)
                            {
                                if (doorCondition is ResourceCondition resourceCondition)
                                {
                                    var hasEnough = currentRun.Inventory.GetItemCount(resourceCondition.RequiredItem) >= resourceCondition.RequiredAmount;
                                    var checkmark = hasEnough ? "✓" : "✗";
                                    var description = $"{checkmark} Requires {resourceCondition.RequiredAmount}x {resourceCondition.RequiredItem?.DisplayName ?? "Unknown"}";
                                    EditorGUILayout.LabelField($"  • {description}", EditorStyles.miniLabel);
                                }
                                else
                                {
                                    EditorGUILayout.LabelField($"  • {doorCondition.GetDescription()}", EditorStyles.miniLabel);
                                }
                            }
                        }
                    }

                    // Show Open Door button if unlocked
                    if (!door.isLocked)
                    {
                        EditorGUILayout.Space(4);
                        if (GUILayout.Button("🚪 Open Door", GUILayout.Height(30)))
                        {
                            currentRun.OpenDoorAndTransition(door);
                            EditorWindow.GetWindow<NetworkRunWindow>().Repaint();
                        }
                    }
                }

                // Show inspect progress or completion button
                if (inspectComponent != null)
                {
                    EditorGUILayout.Space(4);

                    if (inspectComponent.IsComplete)
                    {
                        GUI.backgroundColor = Color.green;
                        if (GUILayout.Button("✓ Complete Task", GUILayout.Height(30)))
                        {
                            inspectComponent.CompleteTask(currentRun, activityInstance);
                            onChanged?.Invoke();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    else
                    {
                        var progress = 1f - (inspectComponent.RemainingDuration / inspectComponent.InspectDuration);
                        var elapsedTime = inspectComponent.InspectDuration - inspectComponent.RemainingDuration;
                        var rect = EditorGUILayout.GetControlRect(false, 20);
                        rect.x += 8;
                        rect.width -= 16;

                        EditorGUI.ProgressBar(rect, progress, $"Inspect: {elapsedTime:F1}s / {inspectComponent.InspectDuration:F1}s");
                    }

                    EditorGUILayout.Space(2);
                }
            };
        }
    }
}
#endif
