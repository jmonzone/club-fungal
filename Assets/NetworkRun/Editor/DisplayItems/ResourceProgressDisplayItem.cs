#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ResourceProgressDisplayItem : UnitDrawerDisplayItem
    {
        public ResourceProgressDisplayItem(ResourceUpdateComponent resourceComponent, ActivityInstance activity)
        {
            condition = () => true;
            color = Color.white;
            drawAction = () =>
            {
                EditorGUILayout.Space(4);

                var unitCount = activity.Units?.Count ?? 0;
                var totalItems = resourceComponent.ItemsPerUpdate * unitCount;
                var itemName = resourceComponent.ItemTemplate?.DisplayName ?? "Unknown";
                var progress = unitCount > 0 ? resourceComponent.Progress : 0f;

                // Show progress bar
                var rect = EditorGUILayout.GetControlRect(false, 20);
                rect.x += 8;
                rect.width -= 16;

                var progressText = unitCount > 0
                    ? $"Collecting {totalItems}x {itemName}: {progress * 100:F0}%"
                    : $"No units assigned";

                EditorGUI.ProgressBar(rect, progress, progressText);
                EditorGUILayout.Space(2);
            };
        }
    }
}
#endif
