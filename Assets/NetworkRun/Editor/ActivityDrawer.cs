#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityDrawer
    {
        public static void DrawList(List<ActivityInstance> activities, RoomTemplate selectedRoom, UnitInstanceService unitInstanceService, System.Action onChanged = null, NetworkRun currentRun = null)
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
                    DrawActivity(activity, selectedRoom, unitInstanceService, onChanged, currentRun);
                }
            }
        }

        private static void DrawActivity(ActivityInstance activity, RoomTemplate selectedRoom, UnitInstanceService unitInstanceService, System.Action onChanged, NetworkRun currentRun = null)
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
                    text: "Delete Activity & Skill",
                    emoji: "🗑️",
                    action: () =>
                    {
                        if (EditorUtility.DisplayDialog("Delete Activity",
                            $"Are you sure you want to delete '{activity.Name}' and its associated skill? This cannot be undone.",
                            "Delete", "Cancel"))
                        {
                            DeleteActivityAndSkill(selectedRoom, activity);
                            onChanged?.Invoke();
                        }
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

            // Check if this activity has InspectComponent - if so, show door info
            bool hasInspectComponent = false;
            InspectComponent inspectComponent = null;
            if (activity.Template?.Components != null)
            {
                foreach (var component in activity.Template.Components)
                {
                    if (component is InspectComponent inspect)
                    {
                        hasInspectComponent = true;
                        inspectComponent = inspect;
                        break;
                    }
                }
            }

            // If this is an inspect activity, show door info and progress
            if (hasInspectComponent && currentRun?.CurrentRoom?.Data?.doors != null)
            {
                var doorInfoItem = new DoorInfoDisplayItem(currentRun, inspectComponent);
                displayItems.Add(doorInfoItem);
            }

            // Add party units grid as display item
            if (unitInstanceService != null && unitInstanceService.Instances != null && unitInstanceService.Instances.Count > 0)
            {
                var partyGridItem = new PartyUnitsGridDisplayItem(activity, selectedRoom, unitInstanceService, onChanged);
                displayItems.Add(partyGridItem);
            }

            // Add unit list as display item
            if (activity.Units != null && activity.Units.Count > 0)
            {
                var unitListItem = new UnitListDisplayItem(activity, selectedRoom, onChanged);
                displayItems.Add(unitListItem);
            }

            // Update display name for inspect activities
            string displayName = activity.Name;
            if (hasInspectComponent && currentRun?.CurrentRoom?.Data?.doors != null)
            {
                var doors = currentRun.CurrentRoom.Data.doors;
                for (int i = 0; i < doors.Count; i++)
                {
                    displayName = $"Door {i + 1}: {activity.Name}";
                    break;
                }
            }

            ItemDrawer.DrawItem(
                icon: icon,
                displayName: displayName,
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
            AssetDatabase.SaveAssets();
        }

        private class PartyUnitsGridDisplayItem : UnitDrawerDisplayItem
        {
            public PartyUnitsGridDisplayItem(ActivityInstance activity, RoomTemplate selectedRoom, UnitInstanceService unitInstanceService, System.Action onChanged)
            {
                condition = () => true;
                color = new Color(0.95f, 0.95f, 1f);
                drawAction = () =>
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField("Party:", EditorStyles.miniLabel);

                    EditorGUILayout.BeginHorizontal();
                    int count = 0;
                    const int itemsPerRow = 6;

                    foreach (var unit in unitInstanceService.Instances)
                    {
                        if (unit == null) continue;

                        if (count > 0 && count % itemsPerRow == 0)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                        }

                        var isInActivity = activity.Units != null && activity.Units.Contains(unit);
                        var buttonColor = isInActivity ? Color.green : GUI.backgroundColor;

                        GUI.backgroundColor = buttonColor;
                        var icon = unit.Species?.Sprite?.texture;
                        var content = icon != null
                            ? new GUIContent(icon, unit.DisplayName)
                            : new GUIContent(unit.DisplayName.Substring(0, System.Math.Min(2, unit.DisplayName.Length)));

                        if (GUILayout.Button(content, GUILayout.Width(40), GUILayout.Height(40)))
                        {
                            if (isInActivity)
                            {
                                RemoveUnitFromActivity(activity, unit, selectedRoom);
                                onChanged?.Invoke();
                            }
                            else
                            {
                                AddUnitToActivity(activity, unit, selectedRoom);
                                onChanged?.Invoke();
                            }
                        }
                        GUI.backgroundColor = Color.white;

                        count++;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);
                };
            }
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

        private static void DeleteActivityAndSkill(RoomTemplate room, ActivityInstance activity)
        {
            RemoveActivityFromRoom(room, activity);

            if (activity.Template != null)
            {
                var templatePath = AssetDatabase.GetAssetPath(activity.Template);

                if (!string.IsNullOrEmpty(templatePath))
                {
                    var folderPath = System.IO.Path.GetDirectoryName(templatePath);

                    if (activity.Template.PrimarySkill != null)
                    {
                        var skillPath = AssetDatabase.GetAssetPath(activity.Template.PrimarySkill);
                        if (!string.IsNullOrEmpty(skillPath))
                        {
                            AssetDatabase.DeleteAsset(skillPath);
                        }
                    }

                    AssetDatabase.DeleteAsset(templatePath);

                    if (!string.IsNullOrEmpty(folderPath) && AssetDatabase.IsValidFolder(folderPath))
                    {
                        AssetDatabase.DeleteAsset(folderPath);
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }

        private class DoorInfoDisplayItem : UnitDrawerDisplayItem
        {
            public DoorInfoDisplayItem(NetworkRun currentRun, InspectComponent inspectComponent)
            {
                condition = () => true;
                color = Color.white;
                drawAction = () =>
                {
                    EditorGUILayout.Space(4);

                    // Show door conditions
                    var doors = currentRun.CurrentRoom.Data.doors;
                    if (doors != null && doors.Count > 0)
                    {
                        var door = doors[0];
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

                    // Show inspect progress
                    if (inspectComponent != null)
                    {
                        EditorGUILayout.Space(4);
                        var progress = 1f - (inspectComponent.RemainingDuration / inspectComponent.InspectDuration);
                        var rect = EditorGUILayout.GetControlRect(false, 20);
                        rect.x += 8;
                        rect.width -= 16;

                        EditorGUI.ProgressBar(rect, progress, $"Inspect: {inspectComponent.RemainingDuration:F1}s / {inspectComponent.InspectDuration:F1}s");
                        EditorGUILayout.Space(2);
                    }
                };
            }
        }
    }
}
#endif
