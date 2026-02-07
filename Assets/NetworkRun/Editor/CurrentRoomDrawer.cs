#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class CurrentRoomDrawer
    {
        private bool isCreatingDoor = false;
        private bool doorIsLocked = false;
        private bool addResourceCondition = false;
        private ItemTemplate requiredItem;
        private int requiredAmount = 1;

        public void Draw(NetworkRun currentRun, List<RoomTemplate> roomTemplates, int selectedRoomIndex)
        {
            if (currentRun == null || roomTemplates == null || roomTemplates.Count == 0) return;

            EditorGUILayout.LabelField("Current Room", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(roomTemplates[selectedRoomIndex].Data.name);

            if (GUILayout.Button("View Room Template"))
            {
                Selection.activeObject = roomTemplates[selectedRoomIndex];
                EditorGUIUtility.PingObject(roomTemplates[selectedRoomIndex]);
            }

            var doors = roomTemplates[selectedRoomIndex].Data.doors;
            if (doors != null && doors.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Doors:", EditorStyles.boldLabel);
                DoorDrawer.DrawList(doors, roomTemplates[selectedRoomIndex], () => EditorWindow.GetWindow<NetworkRunWindow>().Repaint());
            }
            else
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Doors:", EditorStyles.boldLabel);
                DoorDrawer.DrawList(null, roomTemplates[selectedRoomIndex], () => EditorWindow.GetWindow<NetworkRunWindow>().Repaint());
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Add Door to Room"))
            {
                isCreatingDoor = !isCreatingDoor;
            }

            if (isCreatingDoor)
            {
                DrawDoorCreationForm(roomTemplates[selectedRoomIndex]);
            }

            var activities = roomTemplates[selectedRoomIndex].Data.activities;
            if (activities != null && activities.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Activities:", EditorStyles.boldLabel);
                ActivityDrawer.DrawList(activities, roomTemplates[selectedRoomIndex], () => EditorWindow.GetWindow<NetworkRunWindow>().Repaint());
            }
            else
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Activities:", EditorStyles.boldLabel);
                ActivityDrawer.DrawList(null, roomTemplates[selectedRoomIndex], () => EditorWindow.GetWindow<NetworkRunWindow>().Repaint());
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Add Activity to Room"))
            {
                ShowActivityMenu(roomTemplates[selectedRoomIndex]);
            }
        }

        private void ShowActivityMenu(RoomTemplate selectedRoom)
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
                    menu.AddItem(new GUIContent(activity.name), false, () => AddActivityToRoom(selectedRoom, activity));
                }

                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Create New Activity"), false, () => CreateNewActivity(selectedRoom));
            }

            menu.ShowAsContext();
        }

        private void CreateNewActivity(RoomTemplate selectedRoom)
        {
            ActivityCreationWindow.ShowWindow(selectedRoom);
        }

        private void AddActivityToRoom(RoomTemplate selectedRoom, ActivityReference activity)
        {
            if (selectedRoom.Data.activities == null)
            {
                selectedRoom.Data.activities = new List<ActivityInstance>();
            }

            var activityInstance = new ActivityInstance(activity);
            selectedRoom.Data.activities.Add(activityInstance);
            EditorUtility.SetDirty(selectedRoom);
            AssetDatabase.SaveAssets();
        }

        private void DrawDoorCreationForm(RoomTemplate selectedRoom)
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
                CreateDoor(selectedRoom);
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

        public void Update(NetworkRun currentRun, List<RoomTemplate> roomTemplates, int selectedRoomIndex)
        {
            if (currentRun == null || roomTemplates == null || roomTemplates.Count <= selectedRoomIndex) return;

            var currentRoom = roomTemplates[selectedRoomIndex];
            if (currentRoom?.Data?.doors == null) return;

            bool updated = false;
            foreach (var door in currentRoom.Data.doors)
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
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                EditorUtility.SetDirty(currentRoom);
                AssetDatabase.SaveAssets();
            }
        }

        private void CreateDoor(RoomTemplate selectedRoom)
        {
            if (selectedRoom.Data.doors == null)
            {
                selectedRoom.Data.doors = new List<Door>();
            }

            var newDoor = new Door { isLocked = doorIsLocked };

            if (addResourceCondition && requiredItem != null)
            {
                var condition = ScriptableObject.CreateInstance<ResourceCondition>();
                condition.name = $"{requiredItem.DisplayName}x{requiredAmount}Condition";

                var roomPath = AssetDatabase.GetAssetPath(selectedRoom);
                var folderPath = System.IO.Path.GetDirectoryName(roomPath);
                var conditionPath = $"{folderPath}/{condition.name}.asset";
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

            selectedRoom.Data.doors.Add(newDoor);
            EditorUtility.SetDirty(selectedRoom);
            AssetDatabase.SaveAssets();

            isCreatingDoor = false;
            doorIsLocked = false;
            addResourceCondition = false;
            requiredItem = null;
            requiredAmount = 1;

            EditorWindow.GetWindow<NetworkRunWindow>()?.Repaint();
        }
    }
}
#endif
