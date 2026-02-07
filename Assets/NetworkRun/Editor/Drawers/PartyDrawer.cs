#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class PartyDrawer
    {
        public void Draw(System.Collections.Generic.List<UnitInstance> party, GameService gameService)
        {
            EditorGUILayout.LabelField("Party", EditorStyles.boldLabel);
            UnitListDrawer.DrawList(party, gameService.UnitControllerService);
        }
    }
}
#endif
