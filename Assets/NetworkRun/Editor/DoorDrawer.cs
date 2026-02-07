#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class DoorDrawer
    {
        public static void DrawList(List<Door> doors, RoomTemplate selectedRoom, System.Action onChanged = null)
        {
            if (doors == null || doors.Count == 0)
            {
                EditorGUILayout.LabelField("  No doors");
                return;
            }

            for (int i = 0; i < doors.Count; i++)
            {
                DrawDoor(doors[i], i, selectedRoom, onChanged);
            }
        }

        private static void DrawDoor(Door door, int index, RoomTemplate selectedRoom, System.Action onChanged)
        {
            var shortcuts = new List<UnitDrawerItemAction>
            {
                new ActivityItemAction(
                    text: door.isLocked ? "Unlock" : "Lock",
                    emoji: door.isLocked ? "🔓" : "🔒",
                    action: () =>
                    {
                        door.isLocked = !door.isLocked;
                        EditorUtility.SetDirty(selectedRoom);
                        AssetDatabase.SaveAssets();
                        onChanged?.Invoke();
                    }
                )
            };

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
