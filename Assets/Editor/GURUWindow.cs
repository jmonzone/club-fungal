#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

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
    private Editor currentEditor;

    void OnEnable()
    {
        gameService = GURUStyler.LoadAsset<GameService>("GameService");
        unitInstanceService = GURUStyler.LoadAsset<UnitInstanceService>("UnitInstanceService");
        Selection.selectionChanged += Repaint;
        EditorApplication.update += OnEditorUpdate;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        Selection.selectionChanged -= Repaint;
        EditorApplication.update -= OnEditorUpdate;
        SceneView.duringSceneGui -= OnSceneGUI;
        if (currentEditor != null)
        {
            DestroyImmediate(currentEditor);
            currentEditor = null;
        }
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

        // var headerText = "GURU";
        // EditorGUILayout.LabelField(headerText, GURUStyler.LogoStyle);
        // EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        var helpText = "GURU provides Game Utilities & Resources for Unity by providing a custom editor experience to manage game data and services.";
        EditorGUILayout.HelpBox(helpText, MessageType.Info);

        EditorGUILayout.Space(15);

        if (Selection.activeObject is DialogueInteraction dialogue)
        {
            if (currentEditor == null || currentEditor.target != dialogue)
            {
                if (currentEditor != null) DestroyImmediate(currentEditor);
                currentEditor = Editor.CreateEditor(dialogue);
            }
            currentEditor.OnInspectorGUI();
        }
        else
        {
            if (currentEditor != null)
            {
                DestroyImmediate(currentEditor);
                currentEditor = null;
            }
            UnitListDrawer.DrawList(unitInstanceService.Instances);
        }



        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Game Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Game"))
        {
            gameService.ResetJsonFile();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel);

        selectUnitOnMeshClick = EditorGUILayout.Toggle("Select Unit on Mesh Click", selectUnitOnMeshClick);


        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }
    private Vector2 scrollPosition;
}
#endif