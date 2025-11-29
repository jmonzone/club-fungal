using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitControllerService))]
public class UnitControllerServiceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GURUStyler.DrawGuruSection(() =>
        {
            UnitControllerService service = (UnitControllerService)target;

            EditorGUILayout.LabelField("Unit Controllers in Scene:", EditorStyles.boldLabel);

            UnitController[] controllers = Object.FindObjectsOfType<UnitController>();
            if (controllers.Length == 0)
            {
                EditorGUILayout.HelpBox("No Unit Controllers found in the scene.", MessageType.Info);
            }
            else
            {
                DrawControllerList(controllers);
            }
        }, "This section displays all Unit Controllers currently present in the scene, including inactive ones.");
    }

    private void DrawControllerList(UnitController[] controllers)
    {
        foreach (var controller in controllers)
        {
            EditorGUILayout.BeginHorizontal();
            Color originalBG = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            Color originalColor = GUI.color;
            GUI.color = Color.white;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // Draw icon
            if (controller.Instance != null && controller.Instance.Data != null && controller.Instance.Data.Sprite != null)
            {
                GUIContent content = new GUIContent(controller.Instance.Data.Sprite.texture);
                GUILayout.Label(content, GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                GUILayout.Label("No Icon", GUILayout.Width(32), GUILayout.Height(32));
            }
            // Vertical for name and job
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(controller.Instance != null ? controller.Instance.DisplayName : controller.gameObject.name);
            EditorGUILayout.Space(-2); // Reduce space
            GUIStyle jobStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Italic, normal = { textColor = new Color(0.5f, 0.5f, 0.5f) } };
            string jobText = controller.Instance != null && controller.Instance.Job != null ? $"{controller.Instance.Job.Id.ToUpper()}" : "No Job";
            EditorGUILayout.LabelField(jobText, jobStyle);
            if (controller.Instance != null && controller.Instance.IsInParty)
            {
                EditorGUILayout.LabelField("🎉 In Party", jobStyle);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = originalBG;
            GUI.color = originalColor;
            // Buttons next to the box
            EditorGUILayout.BeginVertical(GUILayout.Width(30));
            if (GUILayout.Button("👁", GUILayout.Width(30), GUILayout.Height(20)))
            {
                Selection.activeObject = controller.gameObject;
                EditorGUIUtility.PingObject(controller.gameObject);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            // Space between cards
            EditorGUILayout.Space(5);
        }
    }
}