#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace TheFungalNetwork.Editor
{
    public class ActivityListDrawer
    {
        public static void DrawList(List<ActivityInstance> activities, RoomTemplate selectedRoom, List<UnitInstance> party, System.Action onChanged = null, NetworkRun currentRun = null)
        {
            if (activities == null || activities.Count == 0)
            {
                EditorGUILayout.LabelField("  No activities assigned");
                return;
            }

            foreach (var activity in activities)
            {
                if (activity != null)
                {
                    ActivityDrawer.DrawActivity(activity, selectedRoom, party, onChanged, currentRun);
                }
            }
        }
    }
}
#endif
