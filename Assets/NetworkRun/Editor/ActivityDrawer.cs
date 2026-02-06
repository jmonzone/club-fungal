#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityDrawer
    {
        public static void DrawList(List<ActivityInstance> activities, RoomTemplate selectedRoom, System.Action onChanged = null)
        {
            if (activities == null || activities.Count == 0)
            {
                EditorGUILayout.LabelField("  No activities assigned");
                return;
            }

            foreach (var activity in activities)
            {
                if (activity != null)
                {
                    DrawActivity(activity, selectedRoom, onChanged);
                }
            }
        }

        private static void DrawActivity(ActivityInstance activity, RoomTemplate selectedRoom, System.Action onChanged)
        {
            var menuItems = new List<UnitDrawerItemAction>
            {
                new ActivityItemAction(
                    text: "Remove from Room",
                    emoji: "❌",
                    action: () =>
                    {
                        RemoveActivityFromRoom(selectedRoom, activity);
                        onChanged?.Invoke();
                    }
                ),
                new ActivityItemAction(
                    text: "Select Asset",
                    emoji: "📄",
                    action: () => Selection.activeObject = activity.Template
                )
            };

            Texture icon = null;
            if (activity.Template?.Sprite) icon = activity.Template.Sprite.texture;

            ItemDrawer.DrawItem(
                icon: icon,
                displayName: activity.Name,
                subtitle: null,
                backgroundColor: Color.white,
                shortcuts: null,
                menuItems: menuItems,
                displayItems: null
            );
        }

        private static void RemoveActivityFromRoom(RoomTemplate room, ActivityInstance activity)
        {
            if (room.Data.activities != null && room.Data.activities.Contains(activity))
            {
                room.Data.activities.Remove(activity);
                EditorUtility.SetDirty(room);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
