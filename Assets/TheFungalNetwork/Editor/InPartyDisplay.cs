using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class InPartyDisplay : UnitDrawerDisplayItem
    {
        public InPartyDisplay(bool isInParty, GUIStyle jobStyle)
        {
            condition = () => isInParty;
            color = Color.green;
            drawAction = () => EditorGUILayout.LabelField("🎉 In Party", jobStyle);
        }
    }
}
