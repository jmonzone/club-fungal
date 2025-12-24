using UnityEditor;

namespace TheFungalNetwork.Editor
{
    public class ViewGameObjectAction : UnitDrawerItemAction
    {
        public ViewGameObjectAction(UnitController controller)
        {
            text = "View GameObject";
            emoji = "👁️";
            action = () =>
            {
                Selection.objects = new UnityEngine.Object[] { controller.gameObject };
                EditorGUIUtility.PingObject(controller.gameObject);
                var sceneView = SceneView.lastActiveSceneView;
                sceneView.pivot = controller.transform.position;
                sceneView.size = 10f;
                sceneView.Repaint();
            };
            condition = () => controller != null;
        }
    }
}
