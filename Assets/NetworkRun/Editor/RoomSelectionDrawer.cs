#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class RoomSelectionDrawer
    {
        private string newRoomName = "";

        public void Draw(List<RoomTemplate> roomTemplates, ref int selectedRoomIndex, ref NetworkRun currentRun, System.Action onTemplatesChanged)
        {
            EditorGUILayout.LabelField("Room Selection", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            newRoomName = EditorGUILayout.TextField("New Room Name", newRoomName);
            EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(newRoomName));
            if (GUILayout.Button("Create", GUILayout.Width(80)))
            {
                CreateNewRoomTemplate();
                onTemplatesChanged?.Invoke();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (roomTemplates != null && roomTemplates.Count > 0)
            {
                var roomNames = roomTemplates.Select(r => r.Data.name).ToArray();
                int newIndex = EditorGUILayout.Popup("Select Room", selectedRoomIndex, roomNames);

                if (newIndex != selectedRoomIndex || currentRun == null)
                {
                    selectedRoomIndex = newIndex;
                    var selectedRoom = roomTemplates[selectedRoomIndex];
                    var roomInstance = new RoomInstance(selectedRoom);
                    currentRun = new NetworkRun(roomInstance);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No Room Templates found in project", MessageType.Info);
            }
        }

        private void CreateNewRoomTemplate()
        {
            var newTemplate = ScriptableObject.CreateInstance<RoomTemplate>();
            var path = $"Assets/NetworkRun/Rooms/{newRoomName}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(newTemplate, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = newTemplate;
            newRoomName = "";
        }
    }
}
#endif
