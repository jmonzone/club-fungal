using UnityEditor;

namespace TheFungalNetwork.Editor
{
    public class ViewAssetAction : UnitDrawerItemAction
    {
        public ViewAssetAction(UnitTemplate unitInstance, bool isInitial)
        {
            text = "View Asset";
            emoji = "📁";
            action = () => { Selection.activeObject = unitInstance; EditorGUIUtility.PingObject(unitInstance); };
            condition = () => isInitial;
        }
    }
}
