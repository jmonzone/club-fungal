#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class GURUWindow : EditorWindow
    {
        [MenuItem("Window/GURU")]
        static void ShowWindow()
        {
            GetWindow<GURUWindow>("GURU Inspector");
        }

        private GameService gameService;
        private UnitInstanceService unitInstanceService;
        private bool selectUnitOnMeshClick = true;
        private UnityEditor.Editor currentEditor;
        private GameObject previousSelectedUnit;
        private MonoScript thisScript;

        void OnEnable()
        {
            thisScript = MonoScript.FromScriptableObject(this);
            gameService = GURUStyler.LoadAsset<GameService>("GameService");
            unitInstanceService = GURUStyler.LoadAsset<UnitInstanceService>("UnitInstanceService");
            Selection.selectionChanged += Repaint;
            EditorApplication.update += OnEditorUpdate;
            SceneView.duringSceneGui += OnSceneGUI;
            // UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneOpened;
            // UnityEditor.SceneManagement.EditorSceneManager.sceneClosed += OnSceneClosed;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
            EditorApplication.update -= OnEditorUpdate;
            SceneView.duringSceneGui -= OnSceneGUI;
            // UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= OnSceneOpened;
            // UnityEditor.SceneManagement.EditorSceneManager.sceneClosed -= OnSceneClosed;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode -= OnActiveSceneChanged;
            if (currentEditor != null)
            {
                DestroyImmediate(currentEditor);
                currentEditor = null;
            }
        }

        void OnActiveSceneChanged(UnityEngine.SceneManagement.Scene current, UnityEngine.SceneManagement.Scene next)
        {
            gameService.InitializeSystems();
            Repaint();
        }

        void OnEditorUpdate()
        {
            // Repaint every frame to show live updates during play mode
            Repaint();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!selectUnitOnMeshClick) return;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt && !Event.current.control && !Event.current.shift)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    UnitController unit = hit.collider.GetComponentInParent<UnitController>();
                    if (unit != null)
                    {
                        Selection.activeGameObject = unit.gameObject;
                        Event.current.Use();
                    }
                }
            }
        }

        void OnGUI()
        {
            GUI.backgroundColor = GURUStyler.GuruColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            var headerText = "🍄 GURU of The Fungal Network";
            var headerStyle = new GUIStyle(GURUStyler.LogoStyle) { fontSize = 20 };
            EditorGUILayout.LabelField(headerText, headerStyle, GUILayout.Height(64));

            // Script field
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", thisScript, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.Space(10);

            // var helpText = "GURU provides Game Utilities & Resources for Unity by providing a custom editor experience to manage game data and services.";
            // EditorGUILayout.HelpBox(helpText, MessageType.Info);
            // EditorGUILayout.Space(15);

            if (Selection.activeObject is DialogueInteraction dialogue)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("← Back", GUILayout.Width(60)))
                {
                    Debug.Log("Back button clicked");
                    if (previousSelectedUnit != null)
                    {
                        Debug.Log("Switching back to previous unit");
                        Selection.activeGameObject = previousSelectedUnit;
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (currentEditor == null || currentEditor.target != dialogue)
                {
                    if (currentEditor != null) DestroyImmediate(currentEditor);
                    currentEditor = UnityEditor.Editor.CreateEditor(dialogue);
                }
                currentEditor.OnInspectorGUI();
            }
            else if (Selection.activeObject is ActivityReference activity)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("← Back", GUILayout.Width(60)))
                {
                    if (previousSelectedUnit != null)
                    {
                        Selection.activeGameObject = previousSelectedUnit;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(activity.name, EditorStyles.boldLabel);
                UnitListDrawer.DrawList(activity.Units.Select(unit => unit?.Controller?.Instance), gameService.UnitControllerService);
            }
            else
            {
                GameObject currentSelected = Selection.activeGameObject;

                if (currentSelected != null && currentSelected.GetComponent<UnitController>() != null)
                {
                    previousSelectedUnit = currentSelected;
                }

                if (currentEditor != null)
                {
                    DestroyImmediate(currentEditor);
                    currentEditor = null;
                }

                EditorGUILayout.LabelField("Units", EditorStyles.boldLabel);
                // UnitListDrawer.DrawList(unitControllerService.Instances.Select(instance => instance));
                UnitListDrawer.DrawList(unitInstanceService.Instances, gameService.UnitControllerService);
            }



            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Game Controls", EditorStyles.boldLabel);

            if (GUILayout.Button("Reset Game"))
            {
                gameService.ResetData();
            }

            if (GUILayout.Button("View Local Data"))
            {
                System.Diagnostics.Process.Start(LocalData.GetSaveDataPath());
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel);

            selectUnitOnMeshClick = EditorGUILayout.Toggle("Select Unit on Mesh Click", selectUnitOnMeshClick);


            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }
        private Vector2 scrollPosition;
    }
}
#endif