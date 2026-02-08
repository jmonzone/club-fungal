using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class InteractionDisplay : CardDrawerDisplayItem
    {
        public InteractionDisplay(string interaction, UnitController controller, GUIStyle jobStyle)
        {
            condition = () => !string.IsNullOrEmpty(interaction);
            color = Color.yellow;
            drawAction = () => EditorGUILayout.ObjectField("Interaction", controller?.CurrentInteraction, typeof(UnitInteraction), false);
        }
    }
}
