#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityInstanceWindow : EditorWindow
    {
        private ActivityInstance activityInstance;
        private Vector2 scrollPosition;

        public static void ShowWindow(ActivityInstance instance)
        {
            var window = GetWindow<ActivityInstanceWindow>("Activity Instance");
            window.activityInstance = instance;
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        void OnGUI()
        {
            if (activityInstance == null)
            {
                EditorGUILayout.LabelField("No activity instance selected");
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Activity Instance", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.LabelField("ID", activityInstance.Id);
            EditorGUILayout.LabelField("Name", activityInstance.Name);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Template", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("Reference", activityInstance.Template, typeof(ActivityReference), false);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);

            var data = activityInstance.Data;
            if (data != null)
            {
                EditorGUILayout.TextField("ID", data.id);
                EditorGUILayout.TextField("Name", data.name);

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Units ({data.units?.Count ?? 0})", EditorStyles.boldLabel);

                if (data.units != null && data.units.Count > 0)
                {
                    foreach (var unit in data.units)
                    {
                        if (unit != null)
                        {
                            EditorGUILayout.LabelField($"  - {unit.DisplayName} ({unit.Id})");
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("  No units assigned");
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
