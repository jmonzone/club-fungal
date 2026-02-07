#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class DoorDrawer
    {
        public static void DrawList(List<Door> doors, RoomTemplate selectedRoom, NetworkRun currentRun = null, System.Action onChanged = null)
        {
            if (doors == null || doors.Count == 0)
            {
                EditorGUILayout.LabelField("  No doors");
                return;
            }

            for (int i = 0; i < doors.Count; i++)
            {
                DrawDoor(doors[i], i, selectedRoom, currentRun, onChanged);
            }
        }

        private static void DrawDoor(Door door, int index, RoomTemplate selectedRoom, NetworkRun currentRun, System.Action onChanged)
        {
            var shortcuts = new List<UnitDrawerItemAction>();

            if (!door.isLocked && currentRun != null)
            {
                shortcuts.Add(new ActivityItemAction(
                    text: "Open",
                    emoji: "🚪",
                    action: () =>
                    {
                        currentRun.OpenDoorAndTransition(door);
                        onChanged?.Invoke();
                    }
                ));
            }

            shortcuts.Add(new ActivityItemAction(
                text: door.isLocked ? "Unlock" : "Lock",
                emoji: door.isLocked ? "🔓" : "🔒",
                action: () =>
                {
                    door.isLocked = !door.isLocked;
                    if (selectedRoom != null)
                    {
                        EditorUtility.SetDirty(selectedRoom);
                        AssetDatabase.SaveAssets();
                    }
                    onChanged?.Invoke();
                }
            ));

            var menuItems = new List<UnitDrawerItemAction>
            {
                new ActivityItemAction(
                    text: "Remove Door",
                    emoji: "❌",
                    action: () =>
                    {
                        RemoveDoor(selectedRoom, door);
                        onChanged?.Invoke();
                    }
                )
            };

            var statusText = door.isLocked ? "Locked" : "Unlocked";
            var backgroundColor = door.isLocked ? new Color(1f, 0.8f, 0.8f) : new Color(0.8f, 1f, 0.8f);

            Texture icon = null;
            if (door.conditions != null && door.conditions.Count > 0)
            {
                foreach (var doorCondition in door.conditions)
                {
                    if (doorCondition is ResourceCondition resourceCondition && resourceCondition.RequiredItem?.Sprite != null)
                    {
                        icon = resourceCondition.RequiredItem.Sprite.texture;
                        break;
                    }
                }
            }

            var displayItems = new List<UnitDrawerDisplayItem>();

            if (door.conditions != null && door.conditions.Count > 0)
            {
                var conditionsItem = new ConditionsDisplayItem(door);
                displayItems.Add(conditionsItem);
            }

            // Check if current room has any activities with InspectComponent
            if (currentRun?.CurrentRoom?.Data?.activities != null)
            {
                foreach (var activity in currentRun.CurrentRoom.Data.activities)
                {
                    if (activity?.Template?.Components != null)
                    {
                        foreach (var component in activity.Template.Components)
                        {
                            if (component is InspectComponent inspectComponent)
                            {
                                var inspectItem = new InspectProgressDisplayItem(inspectComponent);
                                displayItems.Add(inspectItem);
                                break;
                            }
                        }
                    }
                }
            }

            ItemDrawer.DrawItem(
                icon: icon,
                displayName: $"Door {index + 1}",
                subtitle: statusText,
                backgroundColor: backgroundColor,
                shortcuts: shortcuts,
                menuItems: menuItems,
                displayItems: displayItems
            );
        }

        private class ConditionsDisplayItem : UnitDrawerDisplayItem
        {
            public ConditionsDisplayItem(Door door)
            {
                condition = () => true;
                color = Color.white;
                drawAction = () =>
                {
                    EditorGUILayout.Space(4);
                    foreach (var doorCondition in door.conditions)
                    {
                        if (doorCondition != null)
                        {
                            if (doorCondition is ResourceCondition resourceCondition)
                            {
                                var description = $"Requires {resourceCondition.RequiredAmount}x {resourceCondition.RequiredItem?.DisplayName ?? "Unknown"}";
                                EditorGUILayout.LabelField($"  • {description}", EditorStyles.miniLabel);
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"  • {doorCondition.GetDescription()}", EditorStyles.miniLabel);
                            }
                        }
                    }
                };
            }
        }

        private class InspectProgressDisplayItem : UnitDrawerDisplayItem
        {
            public InspectProgressDisplayItem(InspectComponent inspectComponent)
            {
                condition = () => true;
                color = Color.white;
                drawAction = () =>
                {
                    EditorGUILayout.Space(4);
                    var progress = 1f - (inspectComponent.RemainingDuration / inspectComponent.InspectDuration);
                    var rect = EditorGUILayout.GetControlRect(false, 20);
                    rect.x += 8;
                    rect.width -= 16;

                    EditorGUI.ProgressBar(rect, progress, $"Inspect: {inspectComponent.RemainingDuration:F1}s / {inspectComponent.InspectDuration:F1}s");
                    EditorGUILayout.Space(2);
                };
            }
        }

        private static void RemoveDoor(RoomTemplate room, Door door)
        {
            if (room.Data.doors != null && room.Data.doors.Contains(door))
            {
                room.Data.doors.Remove(door);
                EditorUtility.SetDirty(room);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
