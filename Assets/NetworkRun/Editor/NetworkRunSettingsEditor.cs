#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkRunSettings))]
public class NetworkRunSettingsEditor : Editor
{
    private SerializedProperty skillsProperty;

    private void OnEnable()
    {
        skillsProperty = serializedObject.FindProperty("skills");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw default inspector for everything except skills
        DrawPropertiesExcluding(serializedObject, "skills");

        EditorGUILayout.Space(10);
        
        // Draw skills section with custom UI
        DrawSkillsSection();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSkillsSection()
    {
        var settings = (NetworkRunSettings)target;

        EditorGUILayout.LabelField("Skills Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // Refresh button to sync with all skills in project
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button("Refresh Skill List", GUILayout.Height(25)))
        {
            RefreshSkillList();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(5);

        // Get all skills
        var skillToggles = settings.GetAllSkillToggles();
        
        if (skillToggles.Count == 0)
        {
            EditorGUILayout.HelpBox("No skills configured. Click 'Refresh Skill List' to load all skills from the project.", MessageType.Info);
            return;
        }

        // Display skills
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        for (int i = 0; i < skillToggles.Count; i++)
        {
            var skillToggle = skillToggles[i];
            if (skillToggle.skill == null) continue;

            EditorGUILayout.BeginHorizontal();

            // Toggle
            var wasEnabled = skillToggle.enabled;
            skillToggle.enabled = EditorGUILayout.Toggle(skillToggle.enabled, GUILayout.Width(20));
            
            if (wasEnabled != skillToggle.enabled)
            {
                EditorUtility.SetDirty(settings);
            }

            // Skill color indicator
            var labelColor = skillToggle.enabled ? new Color(0.7f, 1f, 0.7f) : new Color(0.6f, 0.6f, 0.6f);
            var statusIcon = skillToggle.enabled ? "✓" : "✗";
            
            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = labelColor }
            };

            EditorGUILayout.LabelField(statusIcon, labelStyle, GUILayout.Width(20));

            // Skill reference
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(skillToggle.skill, typeof(Skill), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }

        EditorGUILayout.EndVertical();

        // Quick actions
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Enable All", GUILayout.Height(20)))
        {
            foreach (var toggle in skillToggles)
            {
                toggle.enabled = true;
            }
            EditorUtility.SetDirty(settings);
        }
        
        if (GUILayout.Button("Disable All", GUILayout.Height(20)))
        {
            foreach (var toggle in skillToggles)
            {
                toggle.enabled = false;
            }
            EditorUtility.SetDirty(settings);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void RefreshSkillList()
    {
        var settings = (NetworkRunSettings)target;

        // Find all skills in the project
        var skillGuids = AssetDatabase.FindAssets("t:Skill");
        var allSkills = new List<Skill>();
        
        foreach (var guid in skillGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var skill = AssetDatabase.LoadAssetAtPath<Skill>(path);
            if (skill != null)
            {
                allSkills.Add(skill);
            }
        }

        // Get existing skill toggles
        var existingToggles = settings.GetAllSkillToggles();
        var existingSkills = existingToggles.Where(t => t.skill != null).Select(t => t.skill).ToHashSet();

        // Add new skills that aren't in the list yet
        var newToggles = new List<SkillToggle>();
        
        // Keep existing toggles
        foreach (var toggle in existingToggles)
        {
            if (toggle.skill != null && allSkills.Contains(toggle.skill))
            {
                newToggles.Add(toggle);
            }
        }

        // Add newly discovered skills
        foreach (var skill in allSkills)
        {
            if (!existingSkills.Contains(skill))
            {
                newToggles.Add(new SkillToggle(skill, true)); // New skills enabled by default
            }
        }

        // Sort alphabetically by skill name
        newToggles = newToggles.OrderBy(t => t.skill.name).ToList();

        // Update the settings via serialized property
        skillsProperty.ClearArray();
        for (int i = 0; i < newToggles.Count; i++)
        {
            skillsProperty.InsertArrayElementAtIndex(i);
            var element = skillsProperty.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("skill").objectReferenceValue = newToggles[i].skill;
            element.FindPropertyRelative("enabled").boolValue = newToggles[i].enabled;
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(settings);

        Debug.Log($"Refreshed skill list: {newToggles.Count} skills found");
    }
}
#endif
