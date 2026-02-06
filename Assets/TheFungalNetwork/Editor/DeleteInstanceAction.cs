using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class DeleteInstanceAction : UnitDrawerItemAction
    {
        public DeleteInstanceAction(UnitInstance unitInstance, UnitController controller, bool isInitial)
        {
            text = "Delete Instance";
            emoji = "❌";
            action = () =>
            {
                if (!EditorUtility.DisplayDialog(
                    "Delete Unit Instance",
                    $"Are you sure you want to delete '{unitInstance.DisplayName}' ({unitInstance.Id})?\n\nThis will remove it from data.json and cannot be undone.",
                    "Delete",
                    "Cancel"))
                {
                    return;
                }

                var unitInstanceService = GURUStyler.LoadAsset<UnitInstanceService>(nameof(UnitInstanceService));
                var unitControllerService = GURUStyler.LoadAsset<UnitControllerService>(nameof(UnitControllerService));

                // Remove from services
                unitInstanceService.DeleteUnit(unitInstance);

                // Destroy GameObject if controller exists
                if (controller != null)
                {
                    unitControllerService.RemoveController(controller);
                    Object.DestroyImmediate(controller.gameObject);
                }

                Debug.Log($"Deleted unit instance '{unitInstance.DisplayName}' ({unitInstance.Id})");
            };
            condition = () => !isInitial;
            backgroundColor = Color.red;
        }
    }
}
