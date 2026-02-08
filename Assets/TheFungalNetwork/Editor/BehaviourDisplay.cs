using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class BehaviourDisplay : CardDrawerDisplayItem
    {
        public BehaviourDisplay(string behaviour, GUIStyle jobStyle)
        {
            condition = () => !string.IsNullOrEmpty(behaviour);
            color = Color.yellow;
            drawAction = () => EditorGUILayout.LabelField("Behaviour: " + behaviour, jobStyle);
        }
    }
}
