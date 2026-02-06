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
            var shortcuts = new List<UnitDrawerItemAction>
            {
                new ActivityItemAction(
                    text: "Add Unit",
                    emoji: "➕",
                    action: () =>
                    {
                        ShowAddUnitMenu(activity, selectedRoom, onChanged);
                    }
                )
            };

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
                    text: "Select Template",
                    emoji: "📄",
                    action: () => Selection.activeObject = activity.Template
                ),
                new ActivityItemAction(
                    text: "Select Instance",
                    emoji: "🔍",
                    action: () => ActivityInstanceWindow.ShowWindow(activity)
                )
            };

            Texture icon = null;
            if (activity.Template?.Sprite) icon = activity.Template.Sprite.texture;

            var displayItems = new List<UnitDrawerDisplayItem>();

            // Add unit list as display item
            if (activity.Units != null && activity.Units.Count > 0)
            {
                var unitListItem = new UnitListDisplayItem(activity, selectedRoom, onChanged);
                displayItems.Add(unitListItem);
            }

            ItemDrawer.DrawItem(
                icon: icon,
                displayName: activity.Name,
                subtitle: null,
                backgroundColor: Color.white,
                shortcuts: shortcuts,
                menuItems: menuItems,
                displayItems: displayItems
            );
        }

        private static void DrawUnit(UnitInstance unit, ActivityInstance activity, RoomTemplate selectedRoom, System.Action onChanged)
        {
            var shortcuts = new List<UnitDrawerItemAction>
            {
                new ActivityItemAction(
                    text: "Remove from Activity",
                    emoji: "❌",
                    action: () =>
                    {
                        RemoveUnitFromActivity(activity, unit, selectedRoom);
                        onChanged?.Invoke();
                    }
                )
            };

            Texture icon = unit.Species?.Sprite?.texture;

            ItemDrawer.DrawItem(
                icon: icon,
                displayName: unit.DisplayName,
                subtitle: unit.Job?.Id.ToUpper(),
                backgroundColor: Color.white,
                shortcuts: shortcuts,
                menuItems: null,
                displayItems: null
            );
        }

        private static void RemoveUnitFromActivity(ActivityInstance activity, UnitInstance unit, RoomTemplate room)
        {
            activity.RemoveUnit(unit);
            EditorUtility.SetDirty(room);
            AssetDatabase.SaveAssets();
        }

        private class UnitListDisplayItem : UnitDrawerDisplayItem
        {
            public UnitListDisplayItem(ActivityInstance activity, RoomTemplate selectedRoom, System.Action onChanged)
            {
                condition = () => true;
                color = Color.white;
                drawAction = () =>
                {
                    EditorGUILayout.Space(4);
                    var unitsCopy = new List<UnitInstance>(activity.Units);
                    foreach (var unit in unitsCopy)
                    {
                        if (unit != null)
                        {
                            DrawUnit(unit, activity, selectedRoom, onChanged);
                        }
                    }
                };
            }
        }

        private static void ShowAddUnitMenu(ActivityInstance activity, RoomTemplate selectedRoom, System.Action onChanged)
        {
            var unitService = GURUStyler.LoadAsset<UnitInstanceService>("UnitInstanceService");
            if (unitService == null || unitService.Instances == null || unitService.Instances.Count == 0)
            {
                Debug.LogWarning("No units available");
                return;
            }

            var menu = new GenericMenu();
            foreach (var unit in unitService.Instances)
            {
                if (unit != null)
                {
                    menu.AddItem(new GUIContent(unit.DisplayName), false, () =>
                    {
                        AddUnitToActivity(activity, unit, selectedRoom);
                        onChanged?.Invoke();
                    });
                }
            }
            menu.ShowAsContext();
        }

        private static void AddUnitToActivity(ActivityInstance activity, UnitInstance unit, RoomTemplate room)
        {
            activity.AddUnit(unit);
            EditorUtility.SetDirty(room);
            AssetDatabase.SaveAssets();
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
