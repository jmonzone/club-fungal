using UnityEditor;
using UnityEngine;
using System.Linq;

namespace TheFungalNetwork.Editor
{
    public class CreateControllerAction : UnitDrawerItemAction
    {
        public CreateControllerAction(UnitInstance unitInstance, UnitControllerService service)
        {
            text = "Create Controller";
            emoji = "➕";
            action = () =>
            {
                Debug.Log($"Spawning controller for unit instance '{unitInstance.Id}'");
                var controller = service.SpawnUnit(unitInstance, Vector3.zero, null);
                Selection.objects = new UnityEngine.Object[] { controller.gameObject };
                EditorGUIUtility.PingObject(controller.gameObject);
            };
            condition = () => service.Controllers.All(c => c.Instance != unitInstance);
        }
    }
}
