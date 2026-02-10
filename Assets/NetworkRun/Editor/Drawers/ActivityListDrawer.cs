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

            // Clamp columns to 1-3
            columns = Mathf.Clamp(columns, 1, 3);

            if (columns == 1)
            {
                // Single column layout (original behavior)
                foreach (var activity in activities)
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
                while (index < activities.Count)
                {
                    EditorGUILayout.BeginHorizontal();

                    for (int col = 0; col < columns && index < activities.Count; col++)
                    {
                        var activity = activities[index];

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
