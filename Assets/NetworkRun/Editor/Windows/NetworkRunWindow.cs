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
        private UnlockComponent unlockComponentTemplate;
        private List<RoomTemplate> roomTemplates;
        private int selectedRoomIndex = 0;
        private NetworkRun currentRun;
        private NetworkRunSettings defaultSettings;

        private RoomSelectionDrawer roomSelectionDrawer;
        private CurrentRoomDrawer currentRoomDrawer;
        private PartyDrawer partyDrawer;
        private InventoryDrawer inventoryDrawer;
        private SavedRunsDrawer savedRunsDrawer;

        private bool isRunning = false;
        private int frameCount = 0;
        private const int RepaintInterval = 30;

        void OnEnable()
        {
            thisScript = MonoScript.FromScriptableObject(this);
            gameService = GURUStyler.LoadAsset<GameService>("GameService");
            unitInstanceService = GURUStyler.LoadAsset<UnitInstanceService>("UnitInstanceService");
            unlockComponentTemplate = GURUStyler.LoadAsset<UnlockComponent>("UnlockComponent");
            LoadRoomTemplates();

            // Load default settings asset
            defaultSettings = AssetDatabase.LoadAssetAtPath<NetworkRunSettings>("Assets/NetworkRun/NetworkRunSettings.asset");

            roomSelectionDrawer = new RoomSelectionDrawer();
            currentRoomDrawer = new CurrentRoomDrawer();
            partyDrawer = new PartyDrawer();
            inventoryDrawer = new InventoryDrawer();
            savedRunsDrawer = new SavedRunsDrawer();
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

            // Show settings connection status
            EditorGUILayout.BeginHorizontal();
            string settingsStatus = defaultSettings != null ? $"Settings: Connected ({defaultSettings.name})" : "Settings: Not Connected";
            var statusStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 10, normal = { textColor = defaultSettings != null ? new Color(0.3f, 0.8f, 0.3f) : new Color(1f, 0.5f, 0.5f) } };
            EditorGUILayout.LabelField(settingsStatus, statusStyle, GUILayout.Height(16));

            if (defaultSettings != null && GUILayout.Button("View", GUILayout.Width(50), GUILayout.Height(16)))
            {
                EditorGUIUtility.PingObject(defaultSettings);
                Selection.activeObject = defaultSettings;
            }
            EditorGUILayout.EndHorizontal();

            // Debug mode toggle
            if (defaultSettings != null)
            {
                EditorGUI.BeginChangeCheck();
                bool debugMode = EditorGUILayout.ToggleLeft("Debug Mode", defaultSettings.debugMode, GUILayout.Width(120));
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(defaultSettings, "Toggle Debug Mode");
                    defaultSettings.debugMode = debugMode;
                    if (!debugMode)
                    {
                        defaultSettings.speedMultiplier = 1f;
                    }
                    EditorUtility.SetDirty(defaultSettings);
                }

                // Speed multiplier slider
                EditorGUI.BeginChangeCheck();
                float speedMultiplier = EditorGUILayout.Slider("Speed", defaultSettings.speedMultiplier, 0.1f, 10f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(defaultSettings, "Change Speed Multiplier");
                    defaultSettings.speedMultiplier = speedMultiplier;
                    EditorUtility.SetDirty(defaultSettings);
                }
            }



            if (currentRun != null)
            {
                EditorGUILayout.Space(10);

                DrawPlayPauseButtons();

                EditorGUILayout.Space(10);

                savedRunsDrawer.Draw(LoadRun);

                EditorGUILayout.Space(10);

                if (currentRun.CurrentRoom == null)
                {
                    roomSelectionDrawer.Draw(roomTemplates, ref selectedRoomIndex, ref currentRun, LoadRoomTemplates, (newRun) =>
                    {
                        currentRun = newRun;
                    }, unitInstanceService, unlockComponentTemplate);
                }
                else
                {
                    currentRoomDrawer.Draw(currentRun);

                    EditorGUILayout.Space(10);

                    inventoryDrawer.Draw(currentRun);

                    EditorGUILayout.Space(10);

                    partyDrawer.Draw(currentRun.Party, gameService);
                }
            }
            else
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(10);
                if (GUILayout.Button("Start New Run", GUILayout.Height(40)))
                {
                    CreateNewRun();
                }
                EditorGUILayout.Space(10);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void CreateNewRun()
        {
            selectedRoomIndex = 0;
            LoadRoomTemplates();
            var doorConditions = LoadAllDoorConditions();
            var partySize = defaultSettings != null ? defaultSettings.defaultPartySize : 3;
            var party = GetRandomParty(partySize);
            currentRun = new NetworkRun(doorConditions, party, unlockComponentTemplate, defaultSettings);
            StartRun();
            Repaint();
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

            if (GUILayout.Button("� Inspect", GUILayout.Height(30)))
            {
                NetworkRunInspectorWindow.ShowWindow(currentRun);
            }

            if (GUILayout.Button("�🔄 Start New Run", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Start New Run", "Are you sure? Current run will be lost if not saved.", "Start New", "Cancel"))
                {
                    StopRun();
                    CreateNewRun();
                }
            }

            EditorGUILayout.EndHorizontal();
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
            foreach (var stack in currentRun.Inventory.ItemStacks)
            {
                if (stack?.item != null && !string.IsNullOrEmpty(stack.item.Id))
                {
                    inventoryItems.Add(new InventoryItemData { itemId = stack.item.Id, count = stack.count });
                }
            }

            // Save activities and units
            var savedActivities = new List<SavedActivityData>();
            if (currentRun.CurrentRoom != null && currentRun.CurrentRoom.Data != null)
            {
                var roomData = currentRun.CurrentRoom.Data;
                if (roomData.activities != null)
                {
                    foreach (var activity in roomData.activities)
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
                roomId = currentRun.CurrentRoom?.Data?.id ?? "0",
                currentRoomName = currentRun.CurrentRoom?.Data?.name ?? "Unknown",
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

            // Create a new room instance from saved data
            var activities = new List<ActivityInstance>();
            if (saveData.activities != null)
            {
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

                        activities.Add(activityInstance);
                    }
                }
            }

            var roomData = new RoomData
            {
                id = saveData.roomId,
                name = saveData.currentRoomName,
                doors = new List<Door> { new Door() },
                activities = activities
            };

            var roomInstance = new RoomInstance(roomData);
            var doorConditions = LoadAllDoorConditions();
            var activitiesRefs = LoadAllActivityReferences();
            var party = GetRandomParty(3);
            // currentRun = new NetworkRun(doorConditions, activitiesRefs, party, unlockComponentTemplate);

            // Restore inventory
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

            PerformUpdate();

            Debug.Log($"Network Run loaded from: {filePath}");
            Repaint();
        }

        [System.Serializable]
        private class NetworkRunSaveData
        {
            public List<InventoryItemData> inventoryItems;
            public string roomId;
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

            if (currentRun != null && currentRun.CurrentRoom != null)
            {
                // Update simulation time with speed multiplier
                currentRun.UpdateSimulationTime();
                var roomData = currentRun.CurrentRoom.Data;
                if (roomData?.activities != null)
                {
                    foreach (var activity in roomData.activities)
                    {
                        if (activity != null)
                        {
                            activity.Update(currentRun);
                            hasUpdates = true;
                        }
                    }
                }

                if (roomData?.doors != null)
                {
                    foreach (var door in roomData.doors)
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

                currentRoomDrawer.Update(currentRun);
            }

            return hasUpdates;
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

        private List<UnitInstance> GetRandomParty(int count)
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
                    var originalUnit = availableUnits[randomIndex];

                    // Use service to copy unit with skills preserved
                    var unitCopy = unitInstanceService.CopyUnit(originalUnit, saveData: false, register: false);
                    unitCopy.SetTemplate(originalUnit.Template);

                    party.Add(unitCopy);
                    availableUnits.RemoveAt(randomIndex);
                }
            }
            return party;
        }
    }
}

#endif
