#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class CurrentRoomDrawer
    {
        private PartyUnitsDrawer _partyUnitsDrawer = new PartyUnitsDrawer();
        private bool isCreatingDoor = false;
        private bool doorIsLocked = false;
        private bool addResourceCondition = false;
        private ItemTemplate requiredItem;
        private int requiredAmount = 1;
        private int activityColumns = 1; // Number of columns for activity display (1-3)

        public void Draw(NetworkRun currentRun)
        {
            if (currentRun == null || currentRun.CurrentRoom == null) return;

            var roomData = currentRun.CurrentRoom.Data;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(roomData.name, EditorStyles.boldLabel);
            if (GUILayout.Button("View Room", GUILayout.Width(100)))
            {
                RoomInstanceWindow.ShowWindow(currentRun.CurrentRoom);
            }
            EditorGUILayout.EndHorizontal();

            // Doors are now shown within InspectDoor activities

            // Draw shared available units zone at the top
            EditorGUILayout.Space(5);
            // _partyUnitsDrawer.Draw(currentRun);

            // Column selector
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Columns:", GUILayout.Width(60));
            if (GUILayout.Toggle(activityColumns == 1, "1", EditorStyles.miniButtonLeft, GUILayout.Width(30)))
                activityColumns = 1;
            if (GUILayout.Toggle(activityColumns == 2, "2", EditorStyles.miniButtonMid, GUILayout.Width(30)))
                activityColumns = 2;
            if (GUILayout.Toggle(activityColumns == 3, "3", EditorStyles.miniButtonRight, GUILayout.Width(30)))
                activityColumns = 3;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            ActivityListDrawer.DrawList(roomData.activities, null, currentRun.Party, () => EditorWindow.GetWindow<NetworkRunWindow>().Repaint(), currentRun, activityColumns);


            EditorGUILayout.Space(5);
            if (currentRun.Settings.debugMode && GUILayout.Button("Add Activity to Room"))
            {
                ShowActivityMenu(roomData, currentRun);
            }
        }

        private void ShowActivityMenu(RoomData roomData, NetworkRun currentRun)
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
                    menu.AddItem(new GUIContent(activity.name), false, () => AddActivityToRoom(roomData, activity, currentRun));
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

        private void AddActivityToRoom(RoomData roomData, ActivityReference activity, NetworkRun currentRun)
        {
            // Add to settings.activities
            if (currentRun?.Settings != null)
            {
                if (currentRun.Settings.activities == null)
                {
                    currentRun.Settings.activities = new List<ActivityReference>();
                }
                
                if (!currentRun.Settings.activities.Contains(activity))
                {
                    currentRun.Settings.activities.Add(activity);
                    UnityEditor.EditorUtility.SetDirty(currentRun.Settings);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"Added {activity.name} to settings.activities");
                }
            }
            
            // Also add to room
            if (roomData.activities == null)
            {
                roomData.activities = new List<ActivityInstance>();
            }

            var activityInstance = new ActivityInstance(currentRun, activity);
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
    }
}
#endif
