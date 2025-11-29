using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitControllerService))]
public class UnitControllerServiceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        UnitControllerService service = (UnitControllerService)target;
        service.Initialize();

        GURUStyler.DrawGuruSection(() =>
        {

            EditorGUILayout.LabelField("Unit Controllers in Scene:", EditorStyles.boldLabel);

            if (service.UnitControllers.Count == 0)
            {
                EditorGUILayout.HelpBox("No Unit Controllers found in the scene.", MessageType.Info);
            }
            else
            {
                ControllerListDrawer.DrawList(service.UnitControllers.ToArray());
            }
        }, "This section displays all Unit Controllers currently present in the scene, including inactive ones.");
    }
}