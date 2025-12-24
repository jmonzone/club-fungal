using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ViewInstanceAction : UnitDrawerItemAction
    {
        public ViewInstanceAction(UnitInstance unitInstance)
        {
            text = "View Instance";
            emoji = "👁️";
            action = () => Debug.Log(unitInstance);
            condition = () => true;
        }
    }
}
