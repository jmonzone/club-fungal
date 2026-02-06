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

        void OnEnable()
        {
            thisScript = MonoScript.FromScriptableObject(this);
            gameService = GURUStyler.LoadAsset<GameService>("GameService");
            unitInstanceService = GURUStyler.LoadAsset<UnitInstanceService>("UnitInstanceService");
            LoadRoomTemplates();

            roomSelectionDrawer = new RoomSelectionDrawer();
            currentRoomDrawer = new CurrentRoomDrawer();
            partyDrawer = new PartyDrawer();
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

            roomSelectionDrawer.Draw(roomTemplates, ref selectedRoomIndex, ref currentRun, LoadRoomTemplates);

            EditorGUILayout.Space(10);

            currentRoomDrawer.Draw(currentRun, roomTemplates, selectedRoomIndex);

            EditorGUILayout.Space(10);

            partyDrawer.Draw(unitInstanceService, gameService);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }
    }
}
#endif
