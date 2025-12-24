using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class DeleteInstanceAction : UnitDrawerItemAction
    {
        public DeleteInstanceAction(UnitInstance unitInstance, UnitController controller, bool isInitial)
        {
            text = "Delete Instance";
            emoji = "❌";
            action = () => Debug.LogWarning($"Deleting unit instance '{unitInstance.Id}' is not implemented in editor.");
            condition = () => controller != null && !isInitial;
            backgroundColor = Color.red;
        }
    }
}
