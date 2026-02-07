#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityDrawer
    {
        public static void DrawList(List<ActivityInstance> activities, RoomTemplate selectedRoom, List<UnitInstance> party, System.Action onChanged = null, NetworkRun currentRun = null)
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
                    DrawActivity(activity, selectedRoom, party, onChanged, currentRun);
                }
            }
        }

        private static void DrawActivity(ActivityInstance activity, RoomTemplate selectedRoom, List<UnitInstance> party, System.Action onChanged, NetworkRun currentRun = null)
        {
            var shortcuts = new List<UnitDrawerItemAction>
            {
                new ActivityItemAction(
                    text: "Add Unit",
                    emoji: "➕",
                    action: () =>
                    {
                        ShowAddUnitMenu(activity, selectedRoom, onChanged, currentRun);
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
            bool hasUnlockComponent = false;
            UnlockComponent unlockComponent = null;
            bool hasResourceUpdateComponent = false;
            ResourceUpdateComponent resourceUpdateComponent = null;

            if (activity.Template?.Components != null)
            {
                foreach (var component in activity.Template.Components)
                {
                    if (component is InspectComponent inspect)
                    {
                        hasInspectComponent = true;
                        inspectComponent = inspect;
                    }
                    else if (component is UnlockComponent unlock)
                    {
                        hasUnlockComponent = true;
                        unlockComponent = unlock;
                    }
                    else if (component is ResourceUpdateComponent resourceUpdate)
                    {
                        hasResourceUpdateComponent = true;
                        resourceUpdateComponent = resourceUpdate;
                    }
                }
            }

            // If this is an inspect activity, show door info and progress
            if (hasInspectComponent && currentRun?.CurrentRoom?.Data?.doors != null)
            {
                var doorInfoItem = new DoorInfoDisplayItem(currentRun, inspectComponent, onChanged, activity);
                displayItems.Add(doorInfoItem);
            }

            // If this is an unlock activity, show resource collection progress
            if (hasUnlockComponent && currentRun?.CurrentRoom?.Data?.doors != null)
            {
                var unlockInfoItem = new UnlockInfoDisplayItem(currentRun, unlockComponent, onChanged);
                displayItems.Add(unlockInfoItem);
            }

            // If this activity has ResourceUpdateComponent, show collection progress
            if (hasResourceUpdateComponent && resourceUpdateComponent != null)
            {
                var resourceProgressItem = new ResourceProgressDisplayItem(resourceUpdateComponent, activity, currentRun, onChanged);
                displayItems.Add(resourceProgressItem);
            }

            // Add party units grid as display item (skip for unlock components)
            if (!hasUnlockComponent && party != null && party.Count > 0)
            {
                var partyGridItem = new PartyUnitsGridDisplayItem(activity, selectedRoom, party, onChanged, currentRun);
                displayItems.Add(partyGridItem);
            }

            // Add unit list as display item (skip for unlock components since they show party in UnlockInfoDisplayItem)
            // if (!hasUnlockComponent && activity.Units != null && activity.Units.Count > 0)
            // {
            //     var unitListItem = new UnitListDisplayItem(activity, selectedRoom, onChanged, hasUnlockComponent ? unlockComponent : null, currentRun);
            //     displayItems.Add(unitListItem);
            // }

            // Update display name for inspect or unlock activities
            string displayName = activity.Name;
            if (hasInspectComponent && currentRun?.CurrentRoom?.Data?.doors != null && inspectComponent?.AssignedDoor != null)
            {
                displayName = GetDoorDisplayName(currentRun.CurrentRoom.Data.doors, inspectComponent.AssignedDoor, inspectComponent.DisplayName);
            }
            else if (hasUnlockComponent && currentRun?.CurrentRoom?.Data?.doors != null && unlockComponent?.AssignedDoor != null)
            {
                displayName = GetDoorDisplayName(currentRun.CurrentRoom.Data.doors, unlockComponent.AssignedDoor, unlockComponent.DisplayName);
            }
            else if (activity.Template?.Components != null)
            {
                foreach (var component in activity.Template.Components)
                {
                    var componentType = component.GetType();
                    var displayNameProperty = componentType.GetProperty("DisplayName");
                    if (displayNameProperty != null)
                    {
                        var componentDisplayName = displayNameProperty.GetValue(component) as string;
                        if (!string.IsNullOrEmpty(componentDisplayName))
                        {
                            displayName = componentDisplayName;
                            break;
                        }
                    }
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

        private static string GetDoorDisplayName(List<Door> doors, Door assignedDoor, string componentTypeName)
        {
            if (doors.Count > 1)
            {
                int doorIndex = doors.IndexOf(assignedDoor);
                if (doorIndex >= 0)
                {
                    return $"{componentTypeName} {doorIndex + 1}";
                }
            }

            return $"{componentTypeName}";
        }

        private static void DrawUnit(UnitInstance unit, ActivityInstance activity, RoomTemplate selectedRoom, System.Action onChanged, UnlockComponent unlockComponent = null, NetworkRun currentRun = null)
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

            var displayItems = new List<UnitDrawerDisplayItem>();

            // Add unlock contribution button if this activity has an unlock component
            if (unlockComponent != null && unlockComponent.ResourceCondition != null && currentRun != null)
            {
                var contributeItem = new UnitContributeDisplayItem(unit, unlockComponent, currentRun, onChanged);
                displayItems.Add(contributeItem);
            }

            ItemDrawer.DrawItem(
                icon: icon,
                displayName: unit.DisplayName,
                subtitle: unit.Job?.Id.ToUpper(),
                backgroundColor: Color.white,
                shortcuts: shortcuts,
                menuItems: null,
                displayItems: displayItems.Count > 0 ? displayItems : null
            );
        }

        private static void RemoveUnitFromActivity(ActivityInstance activity, UnitInstance unit, RoomTemplate room)
        {
            activity.RemoveUnit(unit);
            AssetDatabase.SaveAssets();
        }

        private class UnitListDisplayItem : UnitDrawerDisplayItem
        {
            public UnitListDisplayItem(ActivityInstance activity, RoomTemplate selectedRoom, System.Action onChanged, UnlockComponent unlockComponent = null, NetworkRun currentRun = null)
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
                            DrawUnit(unit, activity, selectedRoom, onChanged, unlockComponent, currentRun);
                        }
                    }
                };
            }
        }

        private static void ShowAddUnitMenu(ActivityInstance activity, RoomTemplate selectedRoom, System.Action onChanged, NetworkRun currentRun = null)
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
                        AddUnitToActivity(activity, unit, selectedRoom, currentRun);
                        onChanged?.Invoke();
                    });
                }
            }
            menu.ShowAsContext();
        }

        private static void AddUnitToActivity(ActivityInstance activity, UnitInstance unit, RoomTemplate room, NetworkRun currentRun = null)
        {
            // Get all activities from either room or currentRun
            List<ActivityInstance> allActivities = null;
            if (currentRun?.CurrentRoom?.Data?.activities != null)
            {
                allActivities = currentRun.CurrentRoom.Data.activities;
            }
            else if (room?.Data?.activities != null)
            {
                allActivities = room.Data.activities;
            }

            // Add unit to the activity (logic handles removal from other activities)
            activity.AddUnit(unit, allActivities);
            AssetDatabase.SaveAssets();
        }

        private class PartyUnitsGridDisplayItem : UnitDrawerDisplayItem
        {
            public PartyUnitsGridDisplayItem(ActivityInstance activity, RoomTemplate selectedRoom, List<UnitInstance> party, System.Action onChanged, NetworkRun currentRun = null)
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

                    foreach (var unit in party)
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
                                AddUnitToActivity(activity, unit, selectedRoom, currentRun);
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

        private class UnitContributeDisplayItem : UnitDrawerDisplayItem
        {
            public UnitContributeDisplayItem(UnitInstance unit, UnlockComponent unlockComponent, NetworkRun currentRun, System.Action onChanged)
            {
                condition = () => true;
                color = new Color(1f, 0.95f, 0.85f);
                drawAction = () =>
                {
                    EditorGUILayout.Space(2);

                    var requiredItem = unlockComponent.ResourceCondition.RequiredItem;
                    var requiredAmount = unlockComponent.ResourceCondition.RequiredAmount;
                    var unitItemCount = unit.Inventory.GetItemCount(requiredItem);

                    if (unitItemCount > 0)
                    {
                        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                        var buttonText = $"✓ Use {unitItemCount}x {requiredItem.DisplayName}";

                        if (GUILayout.Button(buttonText, GUILayout.Height(24)))
                        {
                            unlockComponent.ContributeFromUnit(unit);
                            onChanged?.Invoke();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    else
                    {
                        GUI.enabled = false;
                        EditorGUILayout.LabelField($"  No {requiredItem.DisplayName} to contribute", EditorStyles.miniLabel);
                        GUI.enabled = true;
                    }

                    EditorGUILayout.Space(2);
                };
            }
        }
    }
}
#endif
