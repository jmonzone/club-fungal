#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class UnlockInfoDisplayItem : CardDrawerDisplayItem
    {
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
    }
}
#endif
