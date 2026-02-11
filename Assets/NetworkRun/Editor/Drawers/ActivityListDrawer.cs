#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityListDrawer
    {
        public static void DrawList(List<ActivityInstance> activities, RoomTemplate selectedRoom, List<UnitInstance> party, System.Action onChanged = null, NetworkRun currentRun = null, int columns = 1)
        {
            if (activities == null || activities.Count == 0)
            {
                EditorGUILayout.LabelField("  No activities assigned");
                return;
            }

            // Filter activities: show all revealed zones, but only the first unrevealed zone (unless showAllHiddenZones is true)
            // As zones are revealed, the next unrevealed zone automatically becomes visible
            var visibleActivities = new List<ActivityInstance>();
            bool foundUnrevealedZone = false;
            var showAllHidden = currentRun?.Settings?.showAllHiddenZones ?? false;

            Debug.Log($"[ActivityListDrawer] ========== Processing {activities.Count} activities (showAllHidden={showAllHidden}) ==========");

            for (int i = 0; i < activities.Count; i++)
            {
                var activity = activities[i];

                if (activity == null)
                {
                    Debug.LogWarning($"[ActivityListDrawer] Activity at index {i} is NULL");
                    continue;
                }

                Debug.Log($"[ActivityListDrawer] [{i}] Activity: '{activity.Name}', RuntimeComponents: {activity.RuntimeComponents?.Count ?? 0}");

                // Check if this activity has a zone occlusion component
                ZoneOcclusion zoneOcclusion = null;
                if (activity.RuntimeComponents != null)
                {
                    for (int j = 0; j < activity.RuntimeComponents.Count; j++)
                    {
                        var component = activity.RuntimeComponents[j];
                        Debug.Log($"[ActivityListDrawer] [{i}] Component {j}: {component?.GetType().Name ?? "NULL"}");

                        if (component is ZoneOcclusion zo)
                        {
                            zoneOcclusion = zo;
                            Debug.Log($"[ActivityListDrawer] [{i}] Found ZoneOcclusion: IsRevealed={zo.IsRevealed}, Progress={zo.CurrentResourceCount}/{zo.RequiredAmount}");
                        }
                    }
                }

                bool isZoneUnrevealed = zoneOcclusion != null && !zoneOcclusion.IsRevealed;

                if (isZoneUnrevealed)
                {
                    // Show unrevealed zones based on settings
                    if (showAllHidden || !foundUnrevealedZone)
                    {
                        visibleActivities.Add(activity);
                        foundUnrevealedZone = true;
                        Debug.Log($"[ActivityListDrawer] [{i}] ✓ ADDED unrevealed zone: {activity.Name}");
                    }
                    else
                    {
                        Debug.Log($"[ActivityListDrawer] [{i}] ✗ SKIPPED unrevealed zone (already found one): {activity.Name}");
                    }
                }
                else
                {
                    // Always show: activities without occlusion + revealed zones
                    visibleActivities.Add(activity);
                    Debug.Log($"[ActivityListDrawer] [{i}] ✓ ADDED visible activity: {activity.Name}");
                }
            }

            Debug.Log($"[ActivityListDrawer] ========== Final visibleActivities count: {visibleActivities.Count} ==========");

            // Clamp columns to 1-3
            columns = Mathf.Clamp(columns, 1, 3);

            if (columns == 1)
            {
                // Single column layout (original behavior)
                foreach (var activity in visibleActivities)
                {
                    if (activity != null)
                    {
                        ActivityDrawer.DrawActivity(activity, selectedRoom, party, onChanged, currentRun);
                    }
                }
            }
            else
            {
                // Multi-column layout
                float columnWidth = (EditorGUIUtility.currentViewWidth - 20) / columns - 8;

                int index = 0;
                while (index < visibleActivities.Count)
                {
                    EditorGUILayout.BeginHorizontal();

                    for (int col = 0; col < columns && index < visibleActivities.Count; col++)
                    {
                        var activity = visibleActivities[index];

                        EditorGUILayout.BeginVertical(GUILayout.Width(columnWidth), GUILayout.MinWidth(columnWidth), GUILayout.MaxWidth(columnWidth));
                        if (activity != null)
                        {
                            ActivityDrawer.DrawActivity(activity, selectedRoom, party, onChanged, currentRun);
                        }
                        EditorGUILayout.EndVertical();

                        if (col < columns - 1)
                        {
                            GUILayout.Space(8);
                        }

                        index++;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);
                }
            }
        }
    }
}
#endif
