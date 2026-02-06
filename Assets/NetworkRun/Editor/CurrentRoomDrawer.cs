#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class CurrentRoomDrawer
    {
        public void Draw(NetworkRun currentRun, List<RoomTemplate> roomTemplates, int selectedRoomIndex)
        {
            if (currentRun == null || roomTemplates == null || roomTemplates.Count == 0) return;

            EditorGUILayout.LabelField("Current Room", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(roomTemplates[selectedRoomIndex].Data.name);

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
    }
}
#endif
