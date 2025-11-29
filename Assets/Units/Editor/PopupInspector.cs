using UnityEditor;
using UnityEngine;

public class PopupInspector : EditorWindow
{
    private Object targetObject;
    private Vector2 scrollPos;

    public static void Show(Object obj)
    {
        var window = CreateInstance<PopupInspector>();
        window.targetObject = obj;
        window.titleContent = new GUIContent(obj.name);
        window.minSize = new Vector2(400, 300);
        window.ShowUtility();
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        if (targetObject != null)
        {
            SerializedObject serializedObject = new SerializedObject(targetObject);
            serializedObject.Update();
            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                EditorGUILayout.PropertyField(prop, true);
            }
            serializedObject.ApplyModifiedProperties();
        }
        else
        {
            GUILayout.Label("Target is null");
        }
        EditorGUILayout.EndScrollView();
    }
}