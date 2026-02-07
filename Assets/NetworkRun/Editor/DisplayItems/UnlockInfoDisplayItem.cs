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

                    // Show contribute button for network run inventory
                    if (!hasEnough)
                    {
                        var inventoryItemCount = currentRun.Inventory.GetItemCount(unlockComponent.ResourceCondition.RequiredItem);
                        var remainingNeeded = unlockComponent.RequiredAmount - unlockComponent.CurrentResourceCount;
                        var canContribute = Mathf.Min(inventoryItemCount, remainingNeeded);

                        GUI.enabled = canContribute > 0;
                        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);

                        var buttonText = canContribute > 0
                            ? $"✓ Contribute {canContribute}x {resourceName} from Inventory"
                            : $"Collect {resourceName} to contribute";

                        if (GUILayout.Button(buttonText, GUILayout.Height(30)))
                        {
                            // Remove items from network run inventory and contribute to unlock progress
                            for (int i = 0; i < canContribute; i++)
                            {
                                currentRun.Inventory.RemoveItem(unlockComponent.ResourceCondition.RequiredItem);
                            }
                            unlockComponent.ContributeResources(canContribute);
                            Debug.Log($"Contributed {canContribute}x {resourceName} from network run inventory");

                            // Check if door is now unlocked and open it automatically
                            if (unlockComponent.IsUnlocked)
                            {
                                unlockComponent.CompleteTask(currentRun);
                                currentRun.OpenDoorAndTransition(door);
                            }

                            onChanged?.Invoke();
                        }

                        GUI.backgroundColor = Color.white;
                        GUI.enabled = true;
                    }

                    EditorGUILayout.Space(2);
                }
            };
        }
    }
}
#endif
