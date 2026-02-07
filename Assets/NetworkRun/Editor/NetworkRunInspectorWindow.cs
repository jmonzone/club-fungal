#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class NetworkRunInspectorWindow : EditorWindow
    {
        private NetworkRun networkRun;
        private Vector2 scrollPosition;

        public static void ShowWindow(NetworkRun run)
        {
            var window = GetWindow<NetworkRunInspectorWindow>("Network Run Inspector");
            window.networkRun = run;
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        void OnGUI()
        {
            if (networkRun == null)
            {
                EditorGUILayout.HelpBox("No network run to display", MessageType.Info);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Network Run", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Use reflection to display all fields
            var type = typeof(NetworkRun);
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | 
                                       System.Reflection.BindingFlags.NonPublic | 
                                       System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(field.Name, EditorStyles.boldLabel);
                
                var value = field.GetValue(networkRun);
                if (value == null)
                {
                    EditorGUILayout.LabelField("null");
                }
                else if (value is UnityEngine.Object unityObj)
                {
                    EditorGUILayout.ObjectField(unityObj, unityObj.GetType(), false);
                }
                else if (value is System.Collections.IList list)
                {
                    EditorGUILayout.LabelField($"Count: {list.Count}");
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        if (item is UnityEngine.Object unityItem)
                        {
                            EditorGUILayout.ObjectField($"  [{i}]", unityItem, unityItem.GetType(), false);
                        }
                        else if (item != null)
                        {
                            EditorGUILayout.LabelField($"  [{i}] {item}");
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(value.ToString());
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            // Display properties
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | 
                                               System.Reflection.BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField(property.Name + " (Property)", EditorStyles.boldLabel);
                    
                    try
                    {
                        var value = property.GetValue(networkRun);
                        if (value == null)
                        {
                            EditorGUILayout.LabelField("null");
                        }
                        else if (value is UnityEngine.Object unityObj)
                        {
                            EditorGUILayout.ObjectField(unityObj, unityObj.GetType(), false);
                        }
                        else
                        {
                            EditorGUILayout.LabelField(value.ToString());
                        }
                    }
                    catch (System.Exception e)
                    {
                        EditorGUILayout.LabelField($"Error: {e.Message}");
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(5);
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
#endif
