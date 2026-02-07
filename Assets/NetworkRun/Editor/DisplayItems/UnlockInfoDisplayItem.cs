#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class UnlockInfoDisplayItem : UnitDrawerDisplayItem
    {
        public UnlockInfoDisplayItem(NetworkRun currentRun, UnlockComponent unlockComponent, System.Action onChanged)
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

                    // Show Open Door button if enough resources collected, otherwise show progress bar
                    if (hasEnough)
                    {
                        var buttonText = $"🚪 Open Door ({unlockComponent.RequiredAmount}x {resourceName})";
                        if (GUILayout.Button(buttonText, GUILayout.Height(30)))
                        {
                            // Complete task: remove resources and unlock door
                            unlockComponent.CompleteTask(currentRun);
                            currentRun.OpenDoorAndTransition(door);
                            onChanged?.Invoke();
                        }
                    }
                    else
                    {
                        // Show progress bar with resource name
                        var progress = Mathf.Clamp01((float)unlockComponent.CurrentResourceCount / unlockComponent.RequiredAmount);
                        var rect = EditorGUILayout.GetControlRect(false, 20);
                        rect.x += 8;
                        rect.width -= 16;

                        EditorGUI.ProgressBar(rect, progress, $"{resourceName}: {unlockComponent.CurrentResourceCount}/{unlockComponent.RequiredAmount}");
                    }

                    EditorGUILayout.Space(4);

                    // Show party units with contribution options
                    if (currentRun.Party != null && currentRun.Party.Count > 0)
                    {
                        EditorGUILayout.LabelField("Party Units:", EditorStyles.boldLabel);

                        foreach (var unit in currentRun.Party)
                        {
                            if (unit != null)
                            {
                                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                                EditorGUILayout.LabelField(unit.DisplayName, EditorStyles.boldLabel);

                                // Show unit inventory
                                var unitItemCount = unit.Inventory.GetItemCount(unlockComponent.ResourceCondition.RequiredItem);
                                if (unitItemCount > 0)
                                {
                                    EditorGUILayout.LabelField($"📦 {unitItemCount}x {resourceName}", EditorStyles.miniLabel);

                                    // Show contribute button
                                    var remainingNeeded = unlockComponent.RequiredAmount - unlockComponent.CurrentResourceCount;
                                    var canContribute = Mathf.Min(unitItemCount, remainingNeeded);

                                    if (canContribute > 0 && !hasEnough)
                                    {
                                        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                                        if (GUILayout.Button($"✓ Contribute {canContribute}x {resourceName}", GUILayout.Height(24)))
                                        {
                                            unlockComponent.ContributeFromUnit(unit);
                                            onChanged?.Invoke();
                                        }
                                        GUI.backgroundColor = Color.white;
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.LabelField($"No {resourceName} to contribute", EditorStyles.miniLabel);
                                }

                                EditorGUILayout.EndVertical();
                                EditorGUILayout.Space(2);
                            }
                        }
                    }

                    EditorGUILayout.Space(2);
                }
            };
        }
    }
}
#endif
