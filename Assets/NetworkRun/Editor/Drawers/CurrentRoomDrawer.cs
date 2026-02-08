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

            ActivityListDrawer.DrawList(roomData.activities, null, currentRun.Party, () => EditorWindow.GetWindow<NetworkRunWindow>().Repaint(), currentRun);


            EditorGUILayout.Space(5);
            if (currentRun.Settings.debugMode && GUILayout.Button("Add Activity to Room"))
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
    }
}
#endif
