#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class SavedRunsDrawer
    {
        private bool showSavedRuns = false;
        private string[] savedRunFiles;

        public void Draw(System.Action<string> onLoadRun)
        {
            EditorGUILayout.BeginHorizontal();
            showSavedRuns = EditorGUILayout.Foldout(showSavedRuns, "Saved Runs", true, EditorStyles.foldoutHeader);

            if (GUILayout.Button("🔄 Refresh", GUILayout.Width(80)))
            {
                LoadSavedRuns();
            }

            EditorGUILayout.EndHorizontal();

            if (showSavedRuns)
            {
                LoadSavedRuns();

                if (savedRunFiles == null || savedRunFiles.Length == 0)
                {
                    EditorGUILayout.LabelField("  No saved runs found", EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("🗑️ Delete All", GUILayout.Width(100)))
                    {
                        if (EditorUtility.DisplayDialog("Delete All Saves", 
                            $"Are you sure you want to delete all {savedRunFiles.Length} saved runs?", 
                            "Delete All", "Cancel"))
                        {
                            DeleteAllSavedRuns();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(5);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    for (int i = 0; i < savedRunFiles.Length; i++)
                    {
                        var filePath = savedRunFiles[i];
                        EditorGUILayout.BeginHorizontal();

                        var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                        var reverseIndex = savedRunFiles.Length - 1 - i;
                        EditorGUILayout.LabelField($"{reverseIndex}: {fileName}", GUILayout.ExpandWidth(true));

                        if (GUILayout.Button("📂 Load", GUILayout.Width(60)))
                        {
                            onLoadRun?.Invoke(filePath);
                        }

                        if (GUILayout.Button("📁 Show", GUILayout.Width(60)))
                        {
                            EditorUtility.RevealInFinder(filePath);
                        }

                        if (GUILayout.Button("🗑️", GUILayout.Width(30)))
                        {
                            if (EditorUtility.DisplayDialog("Delete Save", $"Delete {fileName}?", "Delete", "Cancel"))
                            {
                                System.IO.File.Delete(filePath);
                                LoadSavedRuns();
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
            }
        }

        private void LoadSavedRuns()
        {
            var savePath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "NetworkRunSaves");
            if (System.IO.Directory.Exists(savePath))
            {
                savedRunFiles = System.IO.Directory.GetFiles(savePath, "*.json")
                    .OrderByDescending(f => System.IO.File.GetLastWriteTime(f))
                    .ToArray();
            }
            else
            {
                savedRunFiles = new string[0];
            }
        }

        private void DeleteAllSavedRuns()
        {
            var savePath = System.IO.Path.Combine(UnityEngine.Application.dataPath, "NetworkRunSaves");
            if (System.IO.Directory.Exists(savePath))
            {
                foreach (var file in System.IO.Directory.GetFiles(savePath, "*.json"))
                {
                    System.IO.File.Delete(file);
                }
                LoadSavedRuns();
            }
        }
    }
}
#endif
