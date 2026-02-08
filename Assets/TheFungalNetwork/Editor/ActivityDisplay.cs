using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ActivityDisplay : CardDrawerDisplayItem
    {
        public ActivityDisplay(ActivityReference activity, GUIStyle jobStyle)
        {
            condition = () => activity != null;
            color = Color.magenta;
            drawAction = () => EditorGUILayout.ObjectField("Activity", activity, typeof(ActivityReference), false);
        }
    }
}
