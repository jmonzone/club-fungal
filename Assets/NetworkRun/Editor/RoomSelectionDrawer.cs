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
        private bool isCreatingNewRoom = false;

        public void Draw(List<RoomTemplate> roomTemplates, ref int selectedRoomIndex, ref NetworkRun currentRun, System.Action onTemplatesChanged, System.Action<NetworkRun> onRunStarted)
        {
            EditorGUILayout.LabelField("Room Selection", EditorStyles.boldLabel);

            if (roomTemplates != null && roomTemplates.Count > 0)
            {
                EditorGUILayout.Space(5);
                
                // Draw each room as a list item
                for (int i = 0; i < roomTemplates.Count; i++)
                {
                    var room = roomTemplates[i];
                    int index = i; // Capture for closure
                    
                    var shortcuts = new List<UnitDrawerItemAction>
                    {
                        new ActivityItemAction("Start Run", "▶", () =>
                        {
                            var roomInstance = new RoomInstance(room);
                            var newRun = new NetworkRun(roomInstance);
                            onRunStarted?.Invoke(newRun);
                        })
                    };
                    
                    var menuItems = new List<UnitDrawerItemAction>
                    {
                        new ActivityItemAction("Select Asset", "", () => Selection.activeObject = room)
                    };
                    
                    ItemDrawer.DrawItem(
                        null, // No icon
                        room.Data.name,
                        room.name,
                        Color.white,
                        shortcuts,
                        menuItems
                    );
                    
                    EditorGUILayout.Space(2);
                }
                
                EditorGUILayout.Space(5);
                
                // Create New Room button
                if (GUILayout.Button("+ Create New Room"))
                {
                    isCreatingNewRoom = true;
                }
                
                if (isCreatingNewRoom)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Create New Room", EditorStyles.boldLabel);
                    newRoomName = EditorGUILayout.TextField("Room Name", newRoomName);
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(newRoomName));
                    if (GUILayout.Button("Create Room"))
                    {
                        CreateNewRoomTemplate();
                        onTemplatesChanged?.Invoke();
                        isCreatingNewRoom = false;
                        newRoomName = "";
                    }
                    EditorGUI.EndDisabledGroup();
                    
                    if (GUILayout.Button("Cancel"))
                    {
                        isCreatingNewRoom = false;
                        newRoomName = "";
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                newRoomName = EditorGUILayout.TextField("New Room Name", newRoomName);
                EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(newRoomName));
                if (GUILayout.Button("Create First Room", GUILayout.Width(120)))
                {
                    CreateNewRoomTemplate();
                    onTemplatesChanged?.Invoke();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("No Room Templates found in project", MessageType.Info);
            }
        }

        private void CreateNewRoomTemplate()
        {
            var newTemplate = ScriptableObject.CreateInstance<RoomTemplate>();
            var path = $"Assets/NetworkRun/Rooms/Data/{newRoomName}.asset";
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
