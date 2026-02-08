#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class CurrentRoomDrawer
    {
        private UnitDropZoneDrawer _dropZoneDrawer = new UnitDropZoneDrawer();
        private bool isCreatingDoor = false;
        private bool doorIsLocked = false;
        private bool addResourceCondition = false;
        private ItemTemplate requiredItem;
        private int requiredAmount = 1;

        public void Draw(NetworkRun currentRun)
        {
            if (currentRun == null || currentRun.CurrentRoom == null) return;

            var roomData = currentRun.CurrentRoom.Data;
            EditorGUILayout.LabelField("Current Room", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(roomData.name);
            if (GUILayout.Button("View Room", GUILayout.Width(100)))
            {
                RoomInstanceWindow.ShowWindow(currentRun.CurrentRoom);
            }
            EditorGUILayout.EndHorizontal();

            // Doors are now shown within InspectDoor activities

            // Draw shared available units zone at the top
            EditorGUILayout.Space(5);
            DrawAvailableUnitsZone(currentRun);

            var activities = roomData.activities;
            if (activities != null && activities.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Activities:", EditorStyles.boldLabel);
                ActivityDrawer.DrawList(activities, null, currentRun.Party, () => EditorWindow.GetWindow<NetworkRunWindow>().Repaint(), currentRun);
            }
            else
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Activities:", EditorStyles.boldLabel);
                ActivityDrawer.DrawList(null, null, currentRun.Party, () => EditorWindow.GetWindow<NetworkRunWindow>().Repaint(), currentRun);
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Add Activity to Room"))
            {
                ShowActivityMenu(roomData);
            }
        }

        private void ShowActivityMenu(RoomData roomData)
        {
            var menu = new GenericMenu();
            var allActivities = AssetDatabase.FindAssets("t:ActivityReference")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ActivityReference>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(activity => activity != null)
                .ToList();

            if (allActivities.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No activities found"));
            }
            else
            {
                foreach (var activity in allActivities)
                {
                    menu.AddItem(new GUIContent(activity.name), false, () => AddActivityToRoom(roomData, activity));
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Create New Activity"), false, () => CreateNewActivity(roomData));
            }

            menu.ShowAsContext();
        }

        private void CreateNewActivity(RoomData roomData)
        {
            ActivityCreationWindow.ShowWindow(roomData);
        }

        private void AddActivityToRoom(RoomData roomData, ActivityReference activity)
        {
            if (roomData.activities == null)
            {
                roomData.activities = new List<ActivityInstance>();
            }

            var activityInstance = new ActivityInstance(activity);
            roomData.activities.Add(activityInstance);
        }

        private void DrawDoorCreationForm(RoomData roomData)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Create New Door", EditorStyles.boldLabel);

            doorIsLocked = EditorGUILayout.Toggle("Is Locked", doorIsLocked);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

            addResourceCondition = EditorGUILayout.Toggle("Add Resource Condition", addResourceCondition);

            if (addResourceCondition)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                requiredItem = (ItemTemplate)EditorGUILayout.ObjectField("Required Item", requiredItem, typeof(ItemTemplate), false);
                requiredAmount = EditorGUILayout.IntField("Required Amount", requiredAmount);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Door"))
            {
                CreateDoor(roomData);
            }

            if (GUILayout.Button("Cancel"))
            {
                isCreatingDoor = false;
                doorIsLocked = false;
                addResourceCondition = false;
                requiredItem = null;
                requiredAmount = 1;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        public void Update(NetworkRun currentRun)
        {
            if (currentRun == null || currentRun.CurrentRoom == null) return;

            var roomData = currentRun.CurrentRoom.Data;
            if (roomData?.doors == null) return;

            foreach (var door in roomData.doors)
            {
                if (door != null && door.isLocked && door.conditions != null && door.conditions.Count > 0)
                {
                    bool allConditionsMet = true;
                    foreach (var condition in door.conditions)
                    {
                        if (condition != null && !condition.IsMet(currentRun.Inventory))
                        {
                            allConditionsMet = false;
                            break;
                        }
                    }

                    if (allConditionsMet)
                    {
                        door.isLocked = false;
                    }
                }
            }
        }

        private void CreateDoor(RoomData roomData)
        {
            if (roomData.doors == null)
            {
                roomData.doors = new List<Door>();
            }

            var newDoor = new Door { isLocked = doorIsLocked };

            if (addResourceCondition && requiredItem != null)
            {
                var condition = ScriptableObject.CreateInstance<ResourceCondition>();
                condition.name = $"{requiredItem.DisplayName}x{requiredAmount}Condition";

                var conditionPath = $"Assets/NetworkRun/Rooms/Conditions/{condition.name}.asset";
                conditionPath = AssetDatabase.GenerateUniqueAssetPath(conditionPath);

                AssetDatabase.CreateAsset(condition, conditionPath);

                var serializedObject = new SerializedObject(condition);
                var requiredItemProperty = serializedObject.FindProperty("requiredItem");
                var requiredAmountProperty = serializedObject.FindProperty("requiredAmount");

                if (requiredItemProperty != null)
                    requiredItemProperty.objectReferenceValue = requiredItem;
                if (requiredAmountProperty != null)
                    requiredAmountProperty.intValue = requiredAmount;

                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();

                newDoor.conditions.Add(condition);
            }

            roomData.doors.Add(newDoor);

            isCreatingDoor = false;
            doorIsLocked = false;
            addResourceCondition = false;
            requiredItem = null;
            requiredAmount = 1;

            EditorWindow.GetWindow<NetworkRunWindow>()?.Repaint();
        }

        private void DrawAvailableUnitsZone(NetworkRun currentRun)
        {
            if (currentRun?.Party == null) return;

            var roomData = currentRun.CurrentRoom?.Data;
            if (roomData?.activities == null) return;

            // Get all units not in any activity
            var unitsInActivities = new HashSet<UnitInstance>();
            foreach (var activity in roomData.activities)
            {
                if (activity?.Units != null)
                {
                    foreach (var unit in activity.Units)
                    {
                        if (unit != null) unitsInActivities.Add(unit);
                    }
                }
            }

            var availableUnits = currentRun.Party.Where(u => u != null && !unitsInActivities.Contains(u)).ToList();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var headerStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            EditorGUILayout.LabelField("Available Units", headerStyle, GUILayout.Height(14));

            var infoStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };
            EditorGUILayout.LabelField("Drag units to activity drop zones below", infoStyle, GUILayout.Height(12));

            // Draw drop zone with available units inside
            _dropZoneDrawer.Draw(
                "🖱️ Drop units here to remove from activities",
                (contentRect) =>
                {
                    if (availableUnits.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        int count = 0;
                        const int itemsPerRow = 3;

                        foreach (var unit in availableUnits)
                        {
                            if (count > 0 && count % itemsPerRow == 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space(2);
                                EditorGUILayout.BeginHorizontal();
                            }

                            // Draw simplified unit card for available units
                            DrawAvailableUnitCard(unit);
                            count++;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(20);
                        var emptyStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            normal = { textColor = new Color(0.5f, 0.5f, 0.5f) },
                            fontStyle = FontStyle.Italic
                        };
                        EditorGUILayout.LabelField("(All units are assigned to activities)", emptyStyle, GUILayout.Height(20));
                        GUILayout.Space(20);
                    }
                },
                (draggedUnit) => unitsInActivities.Contains(draggedUnit),
                (draggedUnit) =>
                {
                    // Remove unit from all activities
                    foreach (var activity in roomData.activities)
                    {
                        if (activity?.Units != null && activity.Units.Contains(draggedUnit))
                        {
                            activity.RemoveUnit(draggedUnit);
                            UnityEditor.AssetDatabase.SaveAssets();
                            break;
                        }
                    }

                    EditorWindow.GetWindow<NetworkRunWindow>()?.Repaint();
                },
                DragAndDropVisualMode.Copy
            );

            EditorGUILayout.EndVertical();
        }

        private void DrawAvailableUnitCard(UnitInstance unit)
        {
            var cardRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(100), GUILayout.Height(80), GUILayout.ExpandWidth(false));

            // Draw drag handle at top
            DrawAvailableUnitDragHandle(unit);

            // Block drop zone from accepting drops on this card
            var evt = Event.current;
            if (cardRect.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    evt.Use();
                }
                else if (evt.type == EventType.DragPerform)
                {
                    evt.Use();
                }
            }

            // Draw icon
            var icon = unit.Species?.Sprite?.texture;
            if (icon != null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(icon, GUILayout.Width(40), GUILayout.Height(40));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(40);
            }

            // Draw name
            var nameStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            EditorGUILayout.LabelField(unit.DisplayName, nameStyle, GUILayout.Height(16));

            // Draw species type
            if (unit.Species?.Type != null)
            {
                var typeStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 8,
                    normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
                };
                EditorGUILayout.LabelField(unit.Species.Type.Id, typeStyle, GUILayout.Height(12));
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAvailableUnitDragHandle(UnitInstance unit)
        {
            var evt = Event.current;

            // Create drag handle area at top of card
            EditorGUILayout.BeginHorizontal(GUILayout.Height(12), GUILayout.Width(90));

            GUILayout.FlexibleSpace();

            var handleStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
            };

            EditorGUILayout.LabelField("⋮⋮", handleStyle, GUILayout.Width(20), GUILayout.Height(12));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            // Get actual rect after layout
            var actualRect = GUILayoutUtility.GetLastRect();

            // Enable drag from handle only
            if (evt.type == EventType.MouseDown && actualRect.Contains(evt.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new UnityEngine.Object[] { };
                DragAndDrop.SetGenericData("UnitInstance", unit);
                DragAndDrop.StartDrag(unit.DisplayName);
                evt.Use();
            }

            // Change cursor when hovering over handle
            if (actualRect.Contains(evt.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(actualRect, MouseCursor.MoveArrow);
            }
        }
    }
}
#endif
