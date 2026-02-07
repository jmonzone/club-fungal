using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class UnitInstanceInspectorWindow : EditorWindow
    {
        private UnitInstance unitInstance;
        private Vector2 scrollPosition;

        public static void ShowWindow(UnitInstance unit)
        {
            var window = GetWindow<UnitInstanceInspectorWindow>("Unit Instance");
            window.unitInstance = unit;
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            if (unitInstance == null)
            {
                EditorGUILayout.HelpBox("No unit instance selected", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Unit Instance", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.TextField("ID", unitInstance.Id ?? "null");
            EditorGUILayout.TextField("Display Name", unitInstance.DisplayName ?? "null");
            EditorGUILayout.ObjectField("Species", unitInstance.Species, typeof(UnitSpecies), false);
            EditorGUILayout.ObjectField("Template", unitInstance.Template, typeof(UnitTemplate), false);
            EditorGUILayout.EnumPopup("Element", unitInstance.Element);
            EditorGUILayout.ObjectField("Job", unitInstance.Job, typeof(Job), false);
            EditorGUILayout.ObjectField("Color Palette", unitInstance.ColorPalette, typeof(ColorPalette), false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Skills", EditorStyles.boldLabel);
            if (unitInstance.Skills != null && unitInstance.Skills.Count > 0)
            {
                foreach (var skill in unitInstance.Skills)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(skill.Key.Id, $"Lvl {skill.Value.Level} - XP: {skill.Value.XP:F1}");
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No skills", EditorStyles.miniLabel);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Friends", EditorStyles.boldLabel);
            if (unitInstance.Friends != null)
            {
                foreach (var friendId in unitInstance.Friends)
                {
                    EditorGUILayout.TextField("Friend ID", friendId);
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
        }
    }
}
