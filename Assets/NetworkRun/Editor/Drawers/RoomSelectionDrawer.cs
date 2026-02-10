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
        private List<RoomTemplate> cachedRoomTemplates;

        public void Draw(List<RoomTemplate> roomTemplates, ref int selectedRoomIndex, ref NetworkRun currentRun, System.Action onTemplatesChanged, System.Action<NetworkRun> onRunStarted, UnitInstanceService unitInstanceService, UnlockComponent unlockTemplate = null)
        {
            cachedRoomTemplates = roomTemplates;
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
                            var doorConditions = LoadAllDoorConditions();
                            var activities = LoadAllActivityReferences();
                            var party = GetRandomParty(unitInstanceService, 3);
                            // var newRun = new NetworkRun(doorConditions, activities, party, unlockTemplate);
                            // onRunStarted?.Invoke(newRun);
                        })
                    };

                    var menuItems = new List<UnitDrawerItemAction>
                    {
                        new ActivityItemAction("Select Asset", "", () => Selection.activeObject = room),
                        new ActivityItemAction("Delete Room", "", () =>
                        {
                            if (EditorUtility.DisplayDialog("Delete Room",
                                $"Are you sure you want to delete '{room.Data.name}'?",
                                "Delete", "Cancel"))
                            {
                                var path = AssetDatabase.GetAssetPath(room);
                                AssetDatabase.DeleteAsset(path);
                                AssetDatabase.SaveAssets();
                                AssetDatabase.Refresh();
                                onTemplatesChanged?.Invoke();
                            }
                        })
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

        private int GetNextRoomId()
        {
            var allRoomTemplates = AssetDatabase.FindAssets("t:RoomTemplate")
                .Select(guid => AssetDatabase.LoadAssetAtPath<RoomTemplate>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(template => template != null && !string.IsNullOrEmpty(template.Data?.id))
                .ToList();

            if (allRoomTemplates.Count == 0)
                return 0;

            int maxId = allRoomTemplates
                .Max(template => int.TryParse(template.Data.id, out int id) ? id : -1);

            return maxId + 1;
        }

        private void CreateNewRoomTemplate()
        {
            var roomId = GetNextRoomId().ToString();

            var door = new Door();
            var allConditions = AssetDatabase.FindAssets("t:DoorCondition")
                .Select(guid => AssetDatabase.LoadAssetAtPath<DoorCondition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(condition => condition != null)
                .ToList();

            if (allConditions.Count > 0)
            {
                var randomCondition = allConditions[UnityEngine.Random.Range(0, allConditions.Count)];
                door.conditions.Add(randomCondition);
            }
            else
            {
                Debug.Log("No DoorCondition assets found in project");
            }

            var doors = new List<Door> { door };

            var activities = new List<ActivityInstance>();
            var allActivityRefs = AssetDatabase.FindAssets("t:ActivityReference")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ActivityReference>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(activityRef => activityRef != null)
                .ToList();

            if (allActivityRefs.Count > 0)
            {
                var randomActivity = allActivityRefs[UnityEngine.Random.Range(0, allActivityRefs.Count)];
                // Create instance without NetworkRun for editor context
                activities.Add(new ActivityInstance(null, randomActivity));
            }

            var newRoom = new RoomData
            {
                id = roomId,
                name = newRoomName,
                doors = doors,
                activities = activities
            };

            var newTemplate = ScriptableObject.CreateInstance<RoomTemplate>();
            newTemplate.SetRoomData(newRoom);

            var path = $"Assets/NetworkRun/Rooms/Data/{newRoomName}.asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(newTemplate, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = newTemplate;
            newRoomName = "";
        }

        private List<DoorCondition> LoadAllDoorConditions()
        {
            return AssetDatabase.FindAssets("t:DoorCondition")
                .Select(guid => AssetDatabase.LoadAssetAtPath<DoorCondition>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(condition => condition != null)
                .ToList();
        }

        private List<ActivityReference> LoadAllActivityReferences()
        {
            return AssetDatabase.FindAssets("t:ActivityReference")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ActivityReference>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(activityRef => activityRef != null)
                .ToList();
        }

        private List<UnitInstance> GetRandomParty(UnitInstanceService unitInstanceService, int count)
        {
            var party = new List<UnitInstance>();
            if (unitInstanceService?.Instances != null && unitInstanceService.Instances.Count > 0)
            {
                // Filter out tree units
                var availableUnits = unitInstanceService.Instances
                    .Where(u => u?.Template?.name != null && !u.Template.name.ToLower().Contains("tree"))
                    .ToList();

                var randomCount = Mathf.Min(count, availableUnits.Count);

                for (int i = 0; i < randomCount; i++)
                {
                    var randomIndex = Random.Range(0, availableUnits.Count);
                    party.Add(availableUnits[randomIndex]);
                    availableUnits.RemoveAt(randomIndex);
                }
            }
            return party;
        }
    }
}
#endif
