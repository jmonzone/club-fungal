#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class PartyDrawer
    {
        public void Draw(UnitInstanceService unitInstanceService, GameService gameService)
        {
            EditorGUILayout.LabelField("Party", EditorStyles.boldLabel);
            UnitListDrawer.DrawList(unitInstanceService.Instances, gameService.UnitControllerService);
        }
    }
}
#endif
