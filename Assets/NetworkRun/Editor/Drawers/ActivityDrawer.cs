#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityDrawer
    {
        private static List<ActivityComponentDrawer> _componentDrawers;

        static ActivityDrawer()
        {
            // Discover all drawer types using reflection
            _componentDrawers = new List<ActivityComponentDrawer>();

            var drawerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && typeof(ActivityComponentDrawer).IsAssignableFrom(type));

            foreach (var drawerType in drawerTypes)
            {
                var drawer = (ActivityComponentDrawer)Activator.CreateInstance(drawerType);
                _componentDrawers.Add(drawer);
            }

            // Sort to ensure DefaultComponentDrawer is last (fallback)
            _componentDrawers = _componentDrawers
                .OrderBy(d => d.ComponentType == typeof(ActivityComponent) ? 1 : 0)
                .ToList();
        }

        public static void DrawActivity(ActivityInstance activity, RoomTemplate selectedRoom, List<UnitInstance> party, System.Action onChanged, NetworkRun currentRun = null)
        {
            var shortcuts = new List<UnitDrawerItemAction>
            {
                // new ActivityItemAction(
                //     text: "Add Unit",
                //     emoji: "➕",
                //     action: () =>
                //     {
                //         ShowAddUnitMenu(activity, selectedRoom, onChanged, currentRun);
                //     }
                // )
            };

            var menuItems = new List<UnitDrawerItemAction>
            {
                // new ActivityItemAction(
                //     text: "Remove from Room",
                //     emoji: "❌",
                //     action: () =>
                //     {
                //         RemoveActivityFromRoom(selectedRoom, activity);
                //         onChanged?.Invoke();
                //     }
                // ),
                // new ActivityItemAction(
                //     text: "Delete Activity & Skill",
                //     emoji: "🗑️",
                //     action: () =>
                //     {
                //         if (EditorUtility.DisplayDialog("Delete Activity",
                //             $"Are you sure you want to delete '{activity.Name}' and its associated skill? This cannot be undone.",
                //             "Delete", "Cancel"))
                //         {
                //             DeleteActivityAndSkill(selectedRoom, activity);
                //             onChanged?.Invoke();
                //         }
                //     }
                // ),
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

            var displayItems = new List<CardDrawerDisplayItem>();

            // Get display items from component drawers
            if (activity.Template?.Components != null)
            {
                foreach (var component in activity.Template.Components)
                {
                    foreach (var drawer in _componentDrawers)
                    {
                        if (drawer.ComponentType.IsAssignableFrom(component.GetType()))
                        {
                            var items = drawer.GetDisplayItems(activity, component, currentRun, onChanged);
                            if (items != null)
                            {
                                displayItems.AddRange(items);
                            }
                            break;
                        }
                    }
                }
            }

            // Show message if no component-specific display items found
            if (displayItems.Count == 0)
            {
                displayItems.Add(new CardDrawerDisplayItem
                {
                    condition = () => true,
                    color = new Color(0.95f, 0.95f, 0.95f),
                    drawAction = () =>
                    {
                        EditorGUILayout.Space(4);
                        var style = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
                        };
                        EditorGUILayout.LabelField("Basic activity with no special components", style);
                        EditorGUILayout.Space(4);
                    }
                });
            }

            // Create unit card drawer function based on components
            Action<UnitInstance> unitCardDrawer = CreateUnitCardDrawer(activity, currentRun, onChanged);

            // Add unified unit drop zone for all activities
            var unitDropZoneItem = new UnifiedUnitDropZoneDisplayItem(
                activity,
                currentRun,
                onChanged,
                unitCardDrawer
            );
            displayItems.Add(unitDropZoneItem);

            ItemDrawer.DrawItem(
                icon: activity.Template.Sprite.texture,
                displayName: activity.Template.Description,
                subtitle: null,
                backgroundColor: Color.white,
                shortcuts: shortcuts,
                menuItems: menuItems,
                displayItems: displayItems
            );
        }

        private static System.Action<UnitInstance> CreateUnitCardDrawer(
            ActivityInstance activity,
            NetworkRun currentRun,
            System.Action onChanged)
        {
            ActivityComponent matchedComponent = null;
            ActivityComponentDrawer matchedDrawer = null;

            if (activity.Template?.Components != null)
            {
                foreach (var component in activity.Template.Components)
                {
                    foreach (var drawer in _componentDrawers)
                    {
                        if (drawer.ComponentType.IsAssignableFrom(component.GetType()))
                        {
                            matchedComponent = component;
                            matchedDrawer = drawer;
                            break;
                        }
                    }
                    if (matchedDrawer != null) break;
                }
            }

            // If no specific component found, use default drawer (last in list)
            if (matchedDrawer == null)
            {
                matchedDrawer = _componentDrawers.Last();
            }

            var finalDrawer = matchedDrawer;
            var finalComponent = matchedComponent;

            return (unit) => finalDrawer.DrawUnitCard(unit, activity, finalComponent, currentRun, onChanged);
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

            var displayItems = new List<CardDrawerDisplayItem>();

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

        private class UnitListDisplayItem : CardDrawerDisplayItem
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

        private class UnitContributeDisplayItem : CardDrawerDisplayItem
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

        private class UnifiedUnitDropZoneDisplayItem : CardDrawerDisplayItem
        {
            private UnitDropZoneDrawer _dropZoneDrawer = new UnitDropZoneDrawer();

            public UnifiedUnitDropZoneDisplayItem(
                ActivityInstance activity,
                NetworkRun currentRun,
                System.Action onChanged,
                System.Action<UnitInstance> unitCardDrawer)
            {
                condition = () => true;
                color = Color.white;
                drawAction = () =>
                {
                    EditorGUILayout.Space(4);

                    var hasUnits = activity.Units != null && activity.Units.Count > 0;

                    _dropZoneDrawer.Draw(
                        isEmpty: !hasUnits,
                        drawContent: (contentRect) =>
                        {
                            EditorGUILayout.BeginHorizontal();
                            int count = 0;
                            const int itemsPerRow = 3;

                            foreach (var unit in activity.Units)
                            {
                                if (unit == null) continue;

                                if (count > 0 && count % itemsPerRow == 0)
                                {
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.Space(2);
                                    EditorGUILayout.BeginHorizontal();
                                }

                                unitCardDrawer(unit);

                                count++;
                            }

                            EditorGUILayout.EndHorizontal();
                        },
                        canDrop: (draggedUnit) => !(activity.Units != null && activity.Units.Contains(draggedUnit)),
                        onDrop: (draggedUnit) =>
                        {
                            var allActivities = currentRun?.CurrentRoom?.Data?.activities;
                            activity.AddUnit(draggedUnit, allActivities);
                            UnityEditor.AssetDatabase.SaveAssets();
                            onChanged?.Invoke();
                        },
                        visualMode: DragAndDropVisualMode.Copy
                    );

                    EditorGUILayout.Space(2);
                };
            }
        }
    }
}
#endif
