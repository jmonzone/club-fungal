#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class NetworkRunWindow : EditorWindow
    {
        [MenuItem("Window/Network Run")]
        static void ShowWindow()
        {
            GetWindow<NetworkRunWindow>("Network Run");
        }

        private MonoScript thisScript;
        private Vector2 scrollPosition;
        private GameService gameService;
        private UnitInstanceService unitInstanceService;
        private List<RoomTemplate> roomTemplates;
        private int selectedRoomIndex = 0;
        private NetworkRun currentRun;

        private RoomSelectionDrawer roomSelectionDrawer;
        private CurrentRoomDrawer currentRoomDrawer;
        private PartyDrawer partyDrawer;
        private InventoryDrawer inventoryDrawer;

        private bool isRunning = false;
        private int frameCount = 0;
        private const int RepaintInterval = 30;

        private bool showSavedRuns = false;
        private string[] savedRunFiles;

        void OnEnable()
        {
            thisScript = MonoScript.FromScriptableObject(this);
            gameService = GURUStyler.LoadAsset<GameService>("GameService");
            unitInstanceService = GURUStyler.LoadAsset<UnitInstanceService>("UnitInstanceService");
            LoadRoomTemplates();

            roomSelectionDrawer = new RoomSelectionDrawer();
            currentRoomDrawer = new CurrentRoomDrawer();
            partyDrawer = new PartyDrawer();
            inventoryDrawer = new InventoryDrawer();
        }

        void OnDisable()
        {
            if (isRunning)
            {
                StopRun();
            }
        }

        private void LoadRoomTemplates()
        {
            roomTemplates = AssetDatabase.FindAssets("t:RoomTemplate")
                .Select(guid => AssetDatabase.LoadAssetAtPath<RoomTemplate>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(template => template != null)
                .ToList();
        }

        void OnGUI()
        {
            GUI.backgroundColor = GURUStyler.GuruColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            var headerText = "Network Run";
            var headerStyle = new GUIStyle(GURUStyler.LogoStyle) { fontSize = 20 };
            EditorGUILayout.LabelField(headerText, headerStyle, GUILayout.Height(64));

            // Script field
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", thisScript, typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUILayout.Space(10);

            DrawPlayPauseButtons();

            EditorGUILayout.Space(10);

            DrawSavedRunsSection();

            EditorGUILayout.Space(10);


            if (currentRun != null)
            {
                if (currentRun.CurrentRoom == null)
                {
                    roomSelectionDrawer.Draw(roomTemplates, ref selectedRoomIndex, ref currentRun, LoadRoomTemplates, (newRun) =>
                    {
                        currentRun = newRun;
                    });
                }
                else
                {
                    inventoryDrawer.Draw(currentRun);

                    EditorGUILayout.Space(10);

                    currentRoomDrawer.Draw(currentRun, roomTemplates, selectedRoomIndex);

                    EditorGUILayout.Space(10);

                    partyDrawer.Draw(unitInstanceService, gameService);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawPlayPauseButtons()
        {
            // if (currentRun == null) return; // Don't show buttons if no run exists
            
            EditorGUILayout.BeginHorizontal();

            if (!isRunning)
            {
                if (GUILayout.Button("▶ Play", GUILayout.Height(30)))
                {
                    StartRun();
                }
            }
            else
            {
                if (GUILayout.Button("⏸ Pause", GUILayout.Height(30)))
                {
                    PauseRun();
                }
            }

            if (GUILayout.Button("💾 Save", GUILayout.Height(30)))
            {
                SaveRun();
            }

            if (GUILayout.Button("🔄 Start New Run", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Start New Run", "Are you sure? Current run will be lost if not saved.", "Start New", "Cancel"))
                {
                    StopRun();
                    
                    // Clear activities from current room
                    if (roomTemplates != null && roomTemplates.Count > selectedRoomIndex)
                    {
                        var currentRoom = roomTemplates[selectedRoomIndex];
                        if (currentRoom?.Data?.activities != null)
                        {
                            currentRoom.Data.activities.Clear();
                        }
                    }
                    
                    currentRun = new NetworkRun();
                    selectedRoomIndex = 0;
                    LoadRoomTemplates();
                    Repaint();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSavedRunsSection()
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
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    for (int i = 0; i < savedRunFiles.Length; i++)
                    {
                        var filePath = savedRunFiles[i];
                        EditorGUILayout.BeginHorizontal();

                        var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                        var reverseIndex = savedRunFiles.Length - 1 - i;
                        EditorGUILayout.LabelField($"{reverseIndex}: {fileName}", GUILayout.ExpandWidth(true));

                        if (GUILayout.Button("� Load", GUILayout.Width(60)))
                        {
                            LoadRun(filePath);
                        }

                        if (GUILayout.Button("�📂 Show", GUILayout.Width(60)))
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
            var savePath = System.IO.Path.Combine(Application.dataPath, "NetworkRunSaves");
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

        private void SaveRun()
        {
            if (currentRun == null) return;

            var savePath = System.IO.Path.Combine(Application.dataPath, "NetworkRunSaves");
            if (!System.IO.Directory.Exists(savePath))
            {
                System.IO.Directory.CreateDirectory(savePath);
            }

            var timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var fileName = $"NetworkRun_{timestamp}.json";
            var fullPath = System.IO.Path.Combine(savePath, fileName);

            // Convert inventory to efficient format
            var inventoryItems = new List<InventoryItemData>();
            var itemCounts = new Dictionary<string, int>();
            foreach (var item in currentRun.Inventory.Items)
            {
                if (item != null && !string.IsNullOrEmpty(item.Id))
                {
                    if (itemCounts.ContainsKey(item.Id))
                    {
                        itemCounts[item.Id]++;
                    }
                    else
                    {
                        itemCounts[item.Id] = 1;
                    }
                }
            }
            foreach (var kvp in itemCounts)
            {
                inventoryItems.Add(new InventoryItemData { itemId = kvp.Key, count = kvp.Value });
            }

            // Save activities and units
            var savedActivities = new List<SavedActivityData>();
            if (roomTemplates != null && roomTemplates.Count > selectedRoomIndex)
            {
                var currentRoom = roomTemplates[selectedRoomIndex];
                if (currentRoom?.Data?.activities != null)
                {
                    foreach (var activity in currentRoom.Data.activities)
                    {
                        if (activity != null)
                        {
                            var unitNames = new List<string>();
                            if (activity.Units != null)
                            {
                                foreach (var unit in activity.Units)
                                {
                                    if (unit != null && unit.Template != null)
                                    {
                                        unitNames.Add(unit.Template.name);
                                    }
                                }
                            }
                            savedActivities.Add(new SavedActivityData
                            {
                                activityId = activity.Id,
                                activityName = activity.Name,
                                unitIds = unitNames
                            });
                        }
                    }
                }
            }

            var saveData = new NetworkRunSaveData
            {
                inventoryItems = inventoryItems,
                currentRoomIndex = selectedRoomIndex,
                currentRoomName = roomTemplates != null && roomTemplates.Count > selectedRoomIndex
                    ? roomTemplates[selectedRoomIndex].Data.name
                    : "Unknown",
                activities = savedActivities
            };

            var json = JsonUtility.ToJson(saveData, true);
            System.IO.File.WriteAllText(fullPath, json);

            Debug.Log($"Network Run saved to: {fullPath}");
            EditorUtility.DisplayDialog("Save Complete", $"Network Run saved to:\n{fullPath}", "OK");
        }

        private void LoadRun(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                EditorUtility.DisplayDialog("Load Failed", "Save file not found.", "OK");
                return;
            }

            var json = System.IO.File.ReadAllText(filePath);
            var saveData = JsonUtility.FromJson<NetworkRunSaveData>(json);

            if (saveData == null)
            {
                EditorUtility.DisplayDialog("Load Failed", "Failed to parse save file.", "OK");
                return;
            }

            selectedRoomIndex = saveData.currentRoomIndex;
            if (selectedRoomIndex >= 0 && selectedRoomIndex < roomTemplates.Count)
            {
                var roomInstance = new RoomInstance(roomTemplates[selectedRoomIndex]);
                // currentRun = new NetworkRun(roomInstance);
                
                // Restore inventory from efficient format
                var inventory = new Inventory();
                if (saveData.inventoryItems != null)
                {
                    foreach (var itemData in saveData.inventoryItems)
                    {
                        var itemTemplates = AssetDatabase.FindAssets($"t:ItemTemplate")
                            .Select(guid => AssetDatabase.LoadAssetAtPath<ItemTemplate>(AssetDatabase.GUIDToAssetPath(guid)))
                            .Where(template => template != null && template.Id == itemData.itemId)
                            .ToList();
                        
                        if (itemTemplates.Count > 0)
                        {
                            var itemTemplate = itemTemplates[0];
                            for (int i = 0; i < itemData.count; i++)
                            {
                                inventory.AddItem(itemTemplate);
                            }
                        }
                    }
                }
                currentRun.SetInventory(inventory);
                
                // Restore activities with units
                var currentRoom = roomTemplates[selectedRoomIndex];
                if (currentRoom?.Data != null && saveData.activities != null)
                {
                    currentRoom.Data.activities.Clear();
                    
                    foreach (var savedActivity in saveData.activities)
                    {
                        var activityAssets = AssetDatabase.FindAssets($"t:ActivityReference")
                            .Select(guid => AssetDatabase.LoadAssetAtPath<ActivityReference>(AssetDatabase.GUIDToAssetPath(guid)))
                            .Where(activityRef => activityRef != null && activityRef.name == savedActivity.activityName)
                            .ToList();
                        
                        if (activityAssets.Count > 0)
                        {
                            var activityRef = activityAssets[0];
                            var activityInstance = new ActivityInstance(activityRef);
                            
                            if (savedActivity.unitIds != null)
                            {
                                foreach (var unitName in savedActivity.unitIds)
                                {
                                    var unitTemplates = AssetDatabase.FindAssets($"t:UnitTemplate")
                                        .Select(guid => AssetDatabase.LoadAssetAtPath<UnitTemplate>(AssetDatabase.GUIDToAssetPath(guid)))
                                        .Where(template => template != null && template.name == unitName)
                                        .ToList();
                                    
                                    if (unitTemplates.Count > 0)
                                    {
                                        var unitTemplate = unitTemplates[0];
                                        var unitInstance = new UnitInstance(unitTemplate.Data);
                                        unitInstance.SetTemplate(unitTemplate);
                                        activityInstance.AddUnit(unitInstance);
                                    }
                                }
                            }
                            
                            currentRoom.Data.activities.Add(activityInstance);
                        }
                    }
                }
                
                PerformUpdate();
                
                Debug.Log($"Network Run loaded from: {filePath}");
                Repaint();
            }
            else
            {
                EditorUtility.DisplayDialog("Load Failed", "Invalid room index in save file.", "OK");
            }
        }

        [System.Serializable]
        private class NetworkRunSaveData
        {
            public List<InventoryItemData> inventoryItems;
            public int currentRoomIndex;
            public string currentRoomName;
            public List<SavedActivityData> activities;
        }

        [System.Serializable]
        private class InventoryItemData
        {
            public string itemId;
            public int count;
        }

        [System.Serializable]
        private class SavedActivityData
        {
            public string activityId;
            public string activityName;
            public List<string> unitIds;
        }

        private void StartRun()
        {
            isRunning = true;
            EditorApplication.update += UpdateLoop;
        }

        private void PauseRun()
        {
            isRunning = false;
            EditorApplication.update -= UpdateLoop;
        }

        private void StopRun()
        {
            isRunning = false;
            EditorApplication.update -= UpdateLoop;
        }

        private void UpdateLoop()
        {
            if (!isRunning) return;

            bool hasUpdates = PerformUpdate();

            if (hasUpdates)
            {
                frameCount++;
                if (frameCount >= RepaintInterval)
                {
                    frameCount = 0;
                    Repaint();
                }
            }
        }

        private bool PerformUpdate()
        {
            bool hasUpdates = false;

            if (currentRun != null && roomTemplates != null && roomTemplates.Count > selectedRoomIndex)
            {
                var currentRoom = roomTemplates[selectedRoomIndex];
                if (currentRoom?.Data?.activities != null)
                {
                    foreach (var activity in currentRoom.Data.activities)
                    {
                        if (activity != null)
                        {
                            activity.Update(currentRun);
                            hasUpdates = true;
                        }
                    }
                }

                if (currentRoom?.Data?.doors != null)
                {
                    foreach (var door in currentRoom.Data.doors)
                    {
                        if (door != null && door.conditions != null && door.conditions.Count() > 0)
                        {
                            bool allConditionsMet = true;
                            foreach (var condition in door.conditions)
                            {
                                if (condition != null && !condition.IsMet(currentRun.Inventory))
                                {
                                    allConditionsMet = false;
                                    break;
                                }
                            }

                            if (door.isLocked != allConditionsMet)
                            {
                                hasUpdates = true;
                            }

                            door.isLocked = !allConditionsMet;
                        }
                    }
                }

                currentRoomDrawer.Update(currentRun, roomTemplates, selectedRoomIndex);
            }

            return hasUpdates;
        }
    }
}
#endif
