#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class RoomInstanceWindow : EditorWindow
    {
        private RoomInstance roomInstance;
        private Vector2 scrollPosition;
        private UnityEditor.Editor roomDataEditor;

        public static void ShowWindow(RoomInstance room)
        {
            var window = GetWindow<RoomInstanceWindow>("Room Inspector");
            window.roomInstance = room;
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        void OnGUI()
        {
            if (roomInstance == null)
            {
                EditorGUILayout.HelpBox("No room instance to display", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Room Instance", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            var roomData = roomInstance.Data;

            EditorGUILayout.LabelField("ID:", EditorStyles.boldLabel);
            EditorGUILayout.TextField(roomData.id);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Name:", EditorStyles.boldLabel);
            roomData.name = EditorGUILayout.TextField(roomData.name);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Doors ({roomData.doors?.Count ?? 0}):", EditorStyles.boldLabel);
            if (roomData.doors != null)
            {
                foreach (var door in roomData.doors)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"Locked: {door.isLocked}");

                    if (door.conditions != null && door.conditions.Count > 0)
                    {
                        EditorGUILayout.LabelField($"Conditions ({door.conditions.Count}):");
                        foreach (var condition in door.conditions)
                        {
                            if (condition != null)
                            {
                                EditorGUILayout.ObjectField(condition.name, condition, typeof(DoorCondition), false);
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Activities ({roomData.activities?.Count ?? 0}):", EditorStyles.boldLabel);
            if (roomData.activities != null)
            {
                foreach (var activity in roomData.activities)
                {
                    if (activity != null)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.LabelField($"Name: {activity.Name}");
                        EditorGUILayout.LabelField($"ID: {activity.Id}");
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.Space(5);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        void OnDestroy()
        {
            if (roomDataEditor != null)
            {
                DestroyImmediate(roomDataEditor);
            }
        }
    }
}
#endif
