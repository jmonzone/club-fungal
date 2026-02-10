#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityCreationWindow : EditorWindow
    {
        private string activityName = "New Activity";
        private string activityDescription = "";
        private Sprite activitySprite;
        private Skill primarySkill;
        private System.Collections.Generic.List<ActivityComponent> components = new System.Collections.Generic.List<ActivityComponent>();
        private RoomData targetRoom;
        private Vector2 scrollPosition;

        private bool isCreatingNewSkill = false;
        private string newSkillName = "New Skill";
        private string newSkillId = "";
        private Sprite newSkillSprite;

        private bool isCreatingNewComponent = false;
        private string newComponentName = "Component";
        private int newComponentSporesPerUpdate = 1;
        private float newComponentUpdateInterval = 1f;
        
        private bool isCreatingNewComponentType = false;
        private string newComponentTypeName = "Custom Component";
        
        // Use SessionState to persist across script reloads
        private const string SESSION_KEY_PENDING_CLASS = "ActivityCreation_PendingClassName";
        private const string SESSION_KEY_PENDING_ACTIVITY = "ActivityCreation_PendingActivityName";
        private const string SESSION_KEY_WAITING = "ActivityCreation_Waiting";
        private const string SESSION_KEY_CONTINUE_ACTIVITY = "ActivityCreation_Continue";
        private const string SESSION_KEY_ACTIVITY_SPRITE = "ActivityCreation_Sprite";
        private const string SESSION_KEY_ACTIVITY_DESCRIPTION = "ActivityCreation_Description";
        private const string SESSION_KEY_PRIMARY_SKILL = "ActivityCreation_Skill";
        private const string SESSION_KEY_COMPONENTS = "ActivityCreation_Components";

        [MenuItem("Tools/Club Fungal/Create New Activity")]
        public static void ShowWindow()
        {
            ShowWindow(null);
        }

        public static void ShowWindow(RoomData room)
        {
            var window = GetWindow<ActivityCreationWindow>("Create Activity");
            window.targetRoom = room;
            window.minSize = new Vector2(400, 200);
            window.ShowUtility();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            Debug.Log("[ActivityCreationWindow] Scripts reloaded callback");
            
            var isWaiting = SessionState.GetBool(SESSION_KEY_WAITING, false);
            var pendingClassName = SessionState.GetString(SESSION_KEY_PENDING_CLASS, "");
            var pendingActivityName = SessionState.GetString(SESSION_KEY_PENDING_ACTIVITY, "");
            
            Debug.Log($"[ActivityCreationWindow] SessionState: waiting={isWaiting}, className={pendingClassName}, activityName={pendingActivityName}");
            
            if (isWaiting && !string.IsNullOrEmpty(pendingClassName))
            {
                Debug.Log($"[ActivityCreationWindow] Detected pending component after reload: {pendingClassName}");
                EditorApplication.delayCall += () =>
                {
                    var asset = CreateComponentAssetAfterCompilation();
                    
                    // Continue with activity creation if flagged
                    if (asset != null && SessionState.GetBool(SESSION_KEY_CONTINUE_ACTIVITY, false))
                    {
                        Debug.Log("[ActivityCreationWindow] Continuing with activity creation...");
                        var component = asset as ActivityComponent;
                        if (component != null)
                        {
                            ContinueActivityCreationWithComponent(component);
                        }
                        else
                        {
                            Debug.LogError($"[ActivityCreationWindow] Asset is not an ActivityComponent: {asset.GetType()}");
                        }
                    }
                };
            }
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Create New Activity", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            activityName = EditorGUILayout.TextField("Activity Name", activityName);
            activityDescription = EditorGUILayout.TextField("Description", activityDescription);
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

            if (isCreatingNewComponentType)
            {
                DrawNewComponentTypeForm();
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
            menu.AddItem(new GUIContent("Create New Component Type (with script)..."), false, () => CreateNewComponentType());

            menu.ShowAsContext();
        }

        private void CreateNewComponent()
        {
            isCreatingNewComponent = true;
            newComponentName = $"{activityName} Component";
            newComponentSporesPerUpdate = 1;
            newComponentUpdateInterval = 1f;
        }

        private void CreateNewComponentType()
        {
            isCreatingNewComponentType = true;
            newComponentTypeName = activityName;
        }

        private string GetComponentClassName(string displayName)
        {
            // Convert "My Component" to "MyComponent"
            var className = displayName.Replace(" ", "").Replace("-", "");
            
            // Ensure it ends with "Component"
            if (!className.EndsWith("Component"))
            {
                className += "Component";
            }
            
            return className;
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

        private void DrawNewComponentTypeForm()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Creating New Component Type (Script + Asset)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("This will create a new C# script and compile it. The script will inherit from ActivityComponent.", MessageType.Info);

            newComponentTypeName = EditorGUILayout.TextField("Component Name", newComponentTypeName);
            
            // Show derived names
            var className = GetComponentClassName(newComponentTypeName);
            EditorGUILayout.LabelField("Script Name:", className + ".cs", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Class Name:", className, EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Menu Path:", "Club Fungal/Activities/Components/" + newComponentTypeName, EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
            {
                isCreatingNewComponentType = false;
                newComponentTypeName = "Custom Component";
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

            if (isCreatingNewComponentType)
            {
                if (CreateComponentTypeScript())
                {
                    // Save activity creation state
                    SaveActivityCreationState();
                    
                    Debug.Log("Component type script and asset will be created automatically. Activity creation will continue after compilation.");
                    Close();
                    return; // Exit - user needs to wait for compilation
                }
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

            if (!string.IsNullOrEmpty(activityDescription))
            {
                var descriptionProperty = serializedObject.FindProperty("description");
                if (descriptionProperty != null)
                {
                    descriptionProperty.stringValue = activityDescription;
                }
            }

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

            // Add to settings.activities - find NetworkRunSettings directly
            var settingsGuids = AssetDatabase.FindAssets("t:NetworkRunSettings");
            if (settingsGuids.Length > 0)
            {
                var settingsPath = AssetDatabase.GUIDToAssetPath(settingsGuids[0]);
                var settings = AssetDatabase.LoadAssetAtPath<NetworkRunSettings>(settingsPath);
                if (settings != null)
                {
                    if (settings.activities == null)
                    {
                        settings.activities = new System.Collections.Generic.List<ActivityReference>();
                    }
                    
                    if (!settings.activities.Contains(newActivity))
                    {
                        settings.activities.Add(newActivity);
                        UnityEditor.EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                        Debug.Log($"Added {newActivity.name} to settings.activities");
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to load NetworkRunSettings from {settingsPath}");
                }
            }
            else
            {
                Debug.LogWarning("No NetworkRunSettings found in project. Activity will not be added to settings.activities list.");
            }

            if (targetRoom != null)
            {
                if (targetRoom.activities == null)
                {
                    targetRoom.activities = new System.Collections.Generic.List<ActivityInstance>();
                }

                // Create a temporary NetworkRun for editor context (components won't be initialized)
                var activityInstance = new ActivityInstance(null, newActivity);
                targetRoom.activities.Add(activityInstance);
            }

            Selection.activeObject = newActivity;
            EditorWindow.GetWindow<NetworkRunWindow>()?.Repaint();
        }

        private bool CreateComponentTypeScript()
        {
            var folderPath = $"Assets/Activities/{activityName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Activities", activityName);
            }

            var className = GetComponentClassName(newComponentTypeName);
            var scriptPath = $"{folderPath}/{className}.cs";
            
            if (System.IO.File.Exists(scriptPath))
            {
                Debug.LogError($"Script already exists at {scriptPath}");
                return false;
            }

            var scriptContent = GenerateComponentScript();
            System.IO.File.WriteAllText(scriptPath, scriptContent);
            
            // Store info for asset creation after compilation using SessionState
            SessionState.SetString(SESSION_KEY_PENDING_CLASS, className);
            SessionState.SetString(SESSION_KEY_PENDING_ACTIVITY, activityName);
            SessionState.SetBool(SESSION_KEY_WAITING, true);
            
            Debug.Log($"[CreateComponentTypeScript] Created script: {scriptPath}");
            Debug.Log($"[CreateComponentTypeScript] Stored in SessionState: className={className}, activityName={activityName}");
            Debug.Log($"[CreateComponentTypeScript] Asset will be auto-created after scripts reload...");
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Created new component script: {scriptPath}\nAsset will be created automatically after compilation...");
            return true;
        }

        [System.Serializable]
        private class StringListWrapper
        {
            public System.Collections.Generic.List<string> items;
        }

        private static UnityEngine.Object CreateComponentAssetAfterCompilation()
        {
            var className = SessionState.GetString(SESSION_KEY_PENDING_CLASS, "");
            var activityName = SessionState.GetString(SESSION_KEY_PENDING_ACTIVITY, "");
            
            Debug.Log($"[CreateComponentAssetAfterCompilation] Started. className={className}, activityName={activityName}");
            
            if (string.IsNullOrEmpty(className) || string.IsNullOrEmpty(activityName))
            {
                Debug.LogWarning($"[CreateComponentAssetAfterCompilation] Pending values are empty, aborting.");
                return null;
            }
            var folderPath = $"Assets/Activities/{activityName}";
            var assetPath = $"{folderPath}/{className}.asset";

            Debug.Log($"[CreateComponentAssetAfterCompilation] Looking for type: {className}");

            // Find the type
            var componentType = System.Type.GetType(className);
            Debug.Log($"[CreateComponentAssetAfterCompilation] Type.GetType result: {componentType}");
            
            if (componentType == null)
            {
                Debug.Log($"[CreateComponentAssetAfterCompilation] Searching assemblies...");
                // Try with assembly-qualified name
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                Debug.Log($"[CreateComponentAssetAfterCompilation] Found {assemblies.Length} assemblies");
                
                foreach (var assembly in assemblies)
                {
                    componentType = assembly.GetType(className);
                    if (componentType != null)
                    {
                        Debug.Log($"[CreateComponentAssetAfterCompilation] Found type in assembly: {assembly.GetName().Name}");
                        break;
                    }
                }
            }

            if (componentType == null)
            {
                Debug.LogError($"[CreateComponentAssetAfterCompilation] Failed to find compiled type: {className}");
                SessionState.SetBool(SESSION_KEY_WAITING, false);
                return null;
            }

            Debug.Log($"[CreateComponentAssetAfterCompilation] Creating ScriptableObject instance of type: {componentType.FullName}");

            // Create the asset
            var asset = ScriptableObject.CreateInstance(componentType);
            if (asset != null)
            {
                Debug.Log($"[CreateComponentAssetAfterCompilation] Instance created successfully");
                asset.name = className;
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                Debug.Log($"[CreateComponentAssetAfterCompilation] Creating asset at: {assetPath}");
                
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Selection.activeObject = asset;
                Debug.Log($"[CreateComponentAssetAfterCompilation] ✓ Created component asset: {assetPath}");
            }
            else
            {
                Debug.LogError($"[CreateComponentAssetAfterCompilation] Failed to create ScriptableObject instance");
            }

            // Clear component creation state (keep activity state for now)
            SessionState.EraseString(SESSION_KEY_PENDING_CLASS);
            SessionState.SetBool(SESSION_KEY_WAITING, false);
            Debug.Log($"[CreateComponentAssetAfterCompilation] Completed and cleared component creation state");
            
            return asset;
        }

        private void SaveActivityCreationState()
        {
            Debug.Log("[SaveActivityCreationState] Saving activity creation state...");
            
            SessionState.SetBool(SESSION_KEY_CONTINUE_ACTIVITY, true);
            
            // Save description
            SessionState.SetString(SESSION_KEY_ACTIVITY_DESCRIPTION, activityDescription);
            
            // Save sprite path
            if (activitySprite != null)
            {
                var spritePath = AssetDatabase.GetAssetPath(activitySprite);
                SessionState.SetString(SESSION_KEY_ACTIVITY_SPRITE, spritePath);
            }
            
            // Save skill path
            if (primarySkill != null)
            {
                var skillPath = AssetDatabase.GetAssetPath(primarySkill);
                SessionState.SetString(SESSION_KEY_PRIMARY_SKILL, skillPath);
            }
            
            // Save component paths as JSON
            if (components != null && components.Count > 0)
            {
                var componentPaths = new System.Collections.Generic.List<string>();
                foreach (var comp in components)
                {
                    if (comp != null)
                    {
                        componentPaths.Add(AssetDatabase.GetAssetPath(comp));
                    }
                }
                var json = JsonUtility.ToJson(new StringListWrapper { items = componentPaths });
                SessionState.SetString(SESSION_KEY_COMPONENTS, json);
            }
            
            Debug.Log($"[SaveActivityCreationState] Saved: description={!string.IsNullOrEmpty(activityDescription)}, sprite={activitySprite != null}, skill={primarySkill != null}, components={components.Count}");
        }

        private static void ContinueActivityCreationWithComponent(ActivityComponent newComponent)
        {
            var activityName = SessionState.GetString(SESSION_KEY_PENDING_ACTIVITY, "");
            
            if (string.IsNullOrEmpty(activityName))
            {
                Debug.LogError("[ContinueActivityCreation] No activity name found in session state");
                return;
            }
            
            Debug.Log($"[ContinueActivityCreation] Creating activity: {activityName}");
            
            // Load description
            var description = SessionState.GetString(SESSION_KEY_ACTIVITY_DESCRIPTION, "");
            
            // Load sprite
            Sprite sprite = null;
            var spritePath = SessionState.GetString(SESSION_KEY_ACTIVITY_SPRITE, "");
            if (!string.IsNullOrEmpty(spritePath))
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            }
            
            // Load skill
            Skill skill = null;
            var skillPath = SessionState.GetString(SESSION_KEY_PRIMARY_SKILL, "");
            if (!string.IsNullOrEmpty(skillPath))
            {
                skill = AssetDatabase.LoadAssetAtPath<Skill>(skillPath);
            }
            
            // Load existing components
            var componentsList = new System.Collections.Generic.List<ActivityComponent>();
            var componentsJson = SessionState.GetString(SESSION_KEY_COMPONENTS, "");
            if (!string.IsNullOrEmpty(componentsJson))
            {
                var wrapper = JsonUtility.FromJson<StringListWrapper>(componentsJson);
                if (wrapper != null && wrapper.items != null)
                {
                    foreach (var componentPath in wrapper.items)
                    {
                        var comp = AssetDatabase.LoadAssetAtPath<ActivityComponent>(componentPath);
                        if (comp != null)
                        {
                            componentsList.Add(comp);
                        }
                    }
                }
            }
            
            // Add the newly created component
            if (newComponent != null)
            {
                componentsList.Add(newComponent);
                Debug.Log($"[ContinueActivityCreation] Added new component: {newComponent.name}");
            }
            
            // Create the activity
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

            if (!string.IsNullOrEmpty(description))
            {
                var descriptionProperty = serializedObject.FindProperty("description");
                if (descriptionProperty != null)
                {
                    descriptionProperty.stringValue = description;
                }
            }

            if (sprite != null)
            {
                var spriteProperty = serializedObject.FindProperty("sprite");
                if (spriteProperty != null)
                {
                    spriteProperty.objectReferenceValue = sprite;
                }
            }

            if (skill != null)
            {
                var skillProperty = serializedObject.FindProperty("primarySkill");
                if (skillProperty != null)
                {
                    skillProperty.objectReferenceValue = skill;
                }
            }

            var componentsProperty = serializedObject.FindProperty("components");
            if (componentsProperty != null)
            {
                componentsProperty.ClearArray();
                foreach (var component in componentsList)
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

            // Add to settings.activities
            var settingsGuids = AssetDatabase.FindAssets("t:NetworkRunSettings");
            if (settingsGuids.Length > 0)
            {
                var settingsPath = AssetDatabase.GUIDToAssetPath(settingsGuids[0]);
                var settings = AssetDatabase.LoadAssetAtPath<NetworkRunSettings>(settingsPath);
                if (settings != null)
                {
                    if (settings.activities == null)
                    {
                        settings.activities = new System.Collections.Generic.List<ActivityReference>();
                    }
                    
                    if (!settings.activities.Contains(newActivity))
                    {
                        settings.activities.Add(newActivity);
                        UnityEditor.EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                        Debug.Log($"[ContinueActivityCreation] Added {newActivity.name} to settings.activities");
                    }
                }
            }

            Selection.activeObject = newActivity;
            Debug.Log($"[ContinueActivityCreation] ✓ Created activity: {path}");
            
            // Clear all session state
            ClearActivityCreationState();
        }

        private static void ClearActivityCreationState()
        {
            SessionState.EraseString(SESSION_KEY_PENDING_ACTIVITY);
            SessionState.SetBool(SESSION_KEY_CONTINUE_ACTIVITY, false);
            SessionState.EraseString(SESSION_KEY_ACTIVITY_DESCRIPTION);
            SessionState.EraseString(SESSION_KEY_ACTIVITY_SPRITE);
            SessionState.EraseString(SESSION_KEY_PRIMARY_SKILL);
            SessionState.EraseString(SESSION_KEY_COMPONENTS);
            Debug.Log("[ClearActivityCreationState] Cleared all session state");
        }

        private string GenerateComponentScript()
        {
            var className = GetComponentClassName(newComponentTypeName);
            var menuPath = newComponentTypeName;
            
            return $@"using UnityEngine;

[CreateAssetMenu(fileName = ""{className}"", menuName = ""Club Fungal/Activities/Components/{menuPath}"")]
public class {className} : ActivityComponent
{{
    // Add your component fields here
    // Example:
    // [SerializeField] private float updateInterval = 1f;
    // [SerializeField] private int value = 1;

    public override void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
    {{
        // Initialize your component here
        Debug.Log($""{className} initialized"");
    }}

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {{
        // Update logic called during network run
        // Example: Process units, update resources, etc.
    }}
}}
";
        }
    }
}
#endif
