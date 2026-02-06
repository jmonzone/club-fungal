#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityCreationWindow : EditorWindow
    {
        private string activityName = "New Activity";
        private Sprite activitySprite;
        private RoomTemplate targetRoom;

        public static void ShowWindow(RoomTemplate room)
        {
            var window = GetWindow<ActivityCreationWindow>("Create Activity");
            window.targetRoom = room;
            window.minSize = new Vector2(400, 200);
            window.ShowUtility();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Create New Activity", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            activityName = EditorGUILayout.TextField("Activity Name", activityName);
            activitySprite = (Sprite)EditorGUILayout.ObjectField("Sprite", activitySprite, typeof(Sprite), false);

            EditorGUILayout.Space(20);

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(activityName));
            if (GUILayout.Button("Create Activity", GUILayout.Height(30)))
            {
                CreateActivity();
                Close();
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
        }

        private void CreateActivity()
        {
            var newActivity = ScriptableObject.CreateInstance<ActivityReference>();
            newActivity.name = activityName;

            var folderPath = $"Assets/Activities/{activityName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Activities", activityName);
            }

            var path = $"{folderPath}/{activityName}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CreateAsset(newActivity, path);

            if (activitySprite != null)
            {
                var serializedObject = new SerializedObject(newActivity);
                var spriteProperty = serializedObject.FindProperty("sprite");
                if (spriteProperty != null)
                {
                    spriteProperty.objectReferenceValue = activitySprite;
                    serializedObject.ApplyModifiedProperties();
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (targetRoom != null)
            {
                if (targetRoom.Data.activities == null)
                {
                    targetRoom.Data.activities = new System.Collections.Generic.List<ActivityInstance>();
                }

                var activityInstance = new ActivityInstance(newActivity);
                targetRoom.Data.activities.Add(activityInstance);
                EditorUtility.SetDirty(targetRoom);
                AssetDatabase.SaveAssets();
            }

            Selection.activeObject = newActivity;
            EditorWindow.GetWindow<NetworkRunWindow>()?.Repaint();
        }
    }
}
#endif
