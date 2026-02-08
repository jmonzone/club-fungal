#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class PartyUnitsDrawer
    {
        private UnitDropZoneDrawer _dropZoneDrawer = new UnitDropZoneDrawer();
        private ResourceUnitCardDrawer _unitCardDrawer = new ResourceUnitCardDrawer();

        public void Draw(NetworkRun currentRun)
        {
            if (currentRun?.Party == null) return;

            var roomData = currentRun.CurrentRoom?.Data;
            if (roomData?.activities == null) return;

            // Get all units not in any activity
            var unitsInActivities = new HashSet<UnitInstance>();
            foreach (var activity in roomData.activities)
            {
                if (activity?.Units != null)
                {
                    foreach (var unit in activity.Units)
                    {
                        if (unit != null) unitsInActivities.Add(unit);
                    }
                }
            }

            var availableUnits = currentRun.Party.Where(u => u != null && !unitsInActivities.Contains(u)).ToList();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(false));

            var headerStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
            EditorGUILayout.LabelField("Available Units", headerStyle, GUILayout.Height(14));

            var infoStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
            };
            EditorGUILayout.LabelField("Drag units to activity drop zones below", infoStyle, GUILayout.Height(12));

            // Draw drop zone with available units inside
            _dropZoneDrawer.Draw(
                "🖱️ Drop units here to remove from activities",
                (contentRect) =>
                {
                    if (availableUnits.Count > 0)
                    {
                        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                        int count = 0;
                        const int itemsPerRow = 3;

                        foreach (var unit in availableUnits)
                        {
                            if (count > 0 && count % itemsPerRow == 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space(2);
                                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                            }

                            _unitCardDrawer.DrawAvailableUnit(unit);
                            count++;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(20);
                        var emptyStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            normal = { textColor = new Color(0.5f, 0.5f, 0.5f) },
                            fontStyle = FontStyle.Italic
                        };
                        EditorGUILayout.LabelField("(All units are assigned to activities)", emptyStyle, GUILayout.Height(20));
                        GUILayout.Space(20);
                    }
                },
                (draggedUnit) => unitsInActivities.Contains(draggedUnit),
                (draggedUnit) =>
                {
                    // Remove unit from all activities
                    foreach (var activity in roomData.activities)
                    {
                        if (activity?.Units != null && activity.Units.Contains(draggedUnit))
                        {
                            activity.RemoveUnit(draggedUnit);
                            UnityEditor.AssetDatabase.SaveAssets();
                            break;
                        }
                    }

                    EditorWindow.GetWindow<NetworkRunWindow>()?.Repaint();
                },
                DragAndDropVisualMode.Copy
            );

            EditorGUILayout.EndVertical();
        }
    }
}
#endif
