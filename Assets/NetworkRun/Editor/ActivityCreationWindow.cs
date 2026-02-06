#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityCreationWindow : EditorWindow
    {
        private string activityName = "New Activity";
        private Sprite activitySprite;
        private Skill primarySkill;
        private System.Collections.Generic.List<ActivityComponent> components = new System.Collections.Generic.List<ActivityComponent>();
        private RoomTemplate targetRoom;
        private Vector2 scrollPosition;

        private bool isCreatingNewSkill = false;
        private string newSkillName = "New Skill";
        private string newSkillId = "";
        private Sprite newSkillSprite;

        private bool isCreatingNewComponent = false;
        private string newComponentName = "Component";
        private int newComponentSporesPerUpdate = 1;
        private float newComponentUpdateInterval = 1f;

        public static void ShowWindow(RoomTemplate room)
        {
            var window = GetWindow<ActivityCreationWindow>("Create Activity");
            window.targetRoom = room;
            window.minSize = new Vector2(400, 200);
            window.ShowUtility();
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Create New Activity", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            activityName = EditorGUILayout.TextField("Activity Name", activityName);
            activitySprite = (Sprite)EditorGUILayout.ObjectField("Sprite", activitySprite, typeof(Sprite), false);

            if (!isCreatingNewSkill)
            {
                DrawPrimarySkillSelector();
            }

            if (isCreatingNewSkill)
            {
                DrawNewSkillForm();
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
            DrawComponentsList();

            if (isCreatingNewComponent)
            {
                DrawNewComponentForm();
            }

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

            EditorGUILayout.EndScrollView();
        }

        private void DrawPrimarySkillSelector()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Primary Skill");

            var displayText = primarySkill != null ? primarySkill.name : "None";
            if (GUILayout.Button(displayText, EditorStyles.popup))
            {
                ShowSkillSelectionMenu();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowSkillSelectionMenu()
        {
            var menu = new GenericMenu();

            var skills = AssetDatabase.FindAssets("t:Skill");
            var skillList = new System.Collections.Generic.List<Skill>();

            foreach (var guid in skills)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var skill = AssetDatabase.LoadAssetAtPath<Skill>(path);
                if (skill != null)
                {
                    skillList.Add(skill);
                }
            }

            if (skillList.Count > 0)
            {
                foreach (var skill in skillList)
                {
                    menu.AddItem(
                        new GUIContent(skill.name),
                        primarySkill == skill,
                        () => { primarySkill = skill; }
                    );
                }

                menu.AddSeparator("");
            }

            menu.AddItem(new GUIContent("Create New Skill..."), false, () => CreateNewSkill());
            menu.AddItem(new GUIContent("Clear"), false, () => { primarySkill = null; });

            menu.ShowAsContext();
        }

        private void CreateNewSkill()
        {
            isCreatingNewSkill = true;
            primarySkill = null;
            newSkillName = activityName;
            newSkillId = activityName;
            newSkillSprite = activitySprite;
        }

        private void DrawNewSkillForm()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Creating New Skill", EditorStyles.boldLabel);

            newSkillName = EditorGUILayout.TextField("Skill Name", newSkillName);
            newSkillId = EditorGUILayout.TextField("Skill ID", newSkillId);
            newSkillSprite = (Sprite)EditorGUILayout.ObjectField("Skill Sprite", newSkillSprite, typeof(Sprite), false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel New Skill"))
            {
                isCreatingNewSkill = false;
                newSkillName = "New Skill";
                newSkillId = "";
                newSkillSprite = null;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private Skill CreateSkillAsset()
        {
            var newSkill = ScriptableObject.CreateInstance<Skill>();
            newSkill.name = newSkillName;

            var folderPath = $"Assets/Activities/{activityName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Activities", activityName);
            }

            var path = $"{folderPath}/{activityName}Skill.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(newSkill, path);

            var serializedObject = new SerializedObject(newSkill);

            var idProperty = serializedObject.FindProperty("id");
            if (idProperty != null)
            {
                idProperty.stringValue = newSkillId;
            }

            if (newSkillSprite != null)
            {
                var spriteProperty = serializedObject.FindProperty("sprite");
                if (spriteProperty != null)
                {
                    spriteProperty.objectReferenceValue = newSkillSprite;
                }
            }

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            return newSkill;
        }

        private void DrawComponentsList()
        {
            if (components.Count == 0)
            {
                EditorGUILayout.LabelField("  No components added", EditorStyles.miniLabel);
            }
            else
            {
                for (int i = 0; i < components.Count; i++)
                {
                    if (components[i] != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"• {components[i].name}");
                        if (GUILayout.Button("❌", GUILayout.Width(30)))
                        {
                            components.RemoveAt(i);
                            i--;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            if (GUILayout.Button("➕ Add Component"))
            {
                ShowAddComponentMenu();
            }
        }

        private void ShowAddComponentMenu()
        {
            var menu = new GenericMenu();

            var componentGuids = AssetDatabase.FindAssets("t:ActivityComponent");
            var componentList = new System.Collections.Generic.List<ActivityComponent>();

            foreach (var guid in componentGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var component = AssetDatabase.LoadAssetAtPath<ActivityComponent>(path);
                if (component != null)
                {
                    componentList.Add(component);
                }
            }

            if (componentList.Count > 0)
            {
                foreach (var component in componentList)
                {
                    menu.AddItem(
                        new GUIContent(component.name),
                        components.Contains(component),
                        () =>
                        {
                            if (!components.Contains(component))
                            {
                                components.Add(component);
                            }
                        }
                    );
                }

                menu.AddSeparator("");
            }

            menu.AddItem(new GUIContent("Create New Resource Update..."), false, () => CreateNewComponent());

            menu.ShowAsContext();
        }

        private void CreateNewComponent()
        {
            isCreatingNewComponent = true;
            newComponentName = $"{activityName} Component";
            newComponentSporesPerUpdate = 1;
            newComponentUpdateInterval = 1f;
        }

        private void DrawNewComponentForm()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Creating New Resource Update Component", EditorStyles.boldLabel);

            newComponentName = EditorGUILayout.TextField("Component Name", newComponentName);
            newComponentSporesPerUpdate = EditorGUILayout.IntField("Spores Per Update", newComponentSporesPerUpdate);
            newComponentUpdateInterval = EditorGUILayout.FloatField("Update Interval", newComponentUpdateInterval);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel New Component"))
            {
                isCreatingNewComponent = false;
                newComponentName = "Component";
                newComponentSporesPerUpdate = 1;
                newComponentUpdateInterval = 1f;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private ActivityComponent CreateComponentAsset()
        {
            var newComponent = ScriptableObject.CreateInstance<ResourceUpdateComponent>();
            newComponent.name = newComponentName;

            var folderPath = $"Assets/Activities/{activityName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Activities", activityName);
            }

            var path = $"{folderPath}/{newComponentName}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(newComponent, path);

            var serializedObject = new SerializedObject(newComponent);

            var sporesProperty = serializedObject.FindProperty("sporesPerUpdate");
            if (sporesProperty != null)
            {
                sporesProperty.intValue = newComponentSporesPerUpdate;
            }

            var intervalProperty = serializedObject.FindProperty("updateInterval");
            if (intervalProperty != null)
            {
                intervalProperty.floatValue = newComponentUpdateInterval;
            }

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();

            return newComponent;
        }

        private void CreateActivity()
        {
            if (isCreatingNewSkill)
            {
                primarySkill = CreateSkillAsset();
                isCreatingNewSkill = false;
            }

            if (isCreatingNewComponent)
            {
                var newComponent = CreateComponentAsset();
                components.Add(newComponent);
                isCreatingNewComponent = false;
            }

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

            var serializedObject = new SerializedObject(newActivity);

            if (activitySprite != null)
            {
                var spriteProperty = serializedObject.FindProperty("sprite");
                if (spriteProperty != null)
                {
                    spriteProperty.objectReferenceValue = activitySprite;
                }
            }

            if (primarySkill != null)
            {
                var skillProperty = serializedObject.FindProperty("primarySkill");
                if (skillProperty != null)
                {
                    skillProperty.objectReferenceValue = primarySkill;
                }
            }

            var componentsProperty = serializedObject.FindProperty("components");
            if (componentsProperty != null)
            {
                componentsProperty.ClearArray();
                foreach (var component in components)
                {
                    if (component != null)
                    {
                        componentsProperty.InsertArrayElementAtIndex(componentsProperty.arraySize);
                        componentsProperty.GetArrayElementAtIndex(componentsProperty.arraySize - 1).objectReferenceValue = component;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

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
