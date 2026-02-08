#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ResourceProgressDisplayItem : CardDrawerDisplayItem
    {
        public ResourceProgressDisplayItem(ResourceUpdateComponent resourceComponent, ActivityInstance activity, NetworkRun currentRun, System.Action onChanged)
        {
            condition = () => true;
            color = Color.white;
            drawAction = () =>
            {
                EditorGUILayout.Space(4);

                var unitCount = activity.Units?.Count ?? 0;
                var itemName = resourceComponent.ItemTemplate?.DisplayName ?? "Unknown";

                // Show what's being collected with type bonuses considered
                var totalItems = resourceComponent.ItemsPerUpdate * unitCount;

                // Calculate average effective interval across all active units
                float totalInterval = 0f;
                int activeCount = 0;

                foreach (var unit in activity.Units)
                {
                    if (unit != null)
                    {
                        totalInterval += resourceComponent.GetEffectiveInterval(unit, activity);
                        activeCount++;
                    }
                }

                float avgInterval = activeCount > 0 ? totalInterval / activeCount : resourceComponent.UpdateInterval;
                EditorGUILayout.LabelField($"⏱ Each cycle: {totalItems}x {itemName} (~{avgInterval:F1}s avg)", EditorStyles.miniLabel);

                EditorGUILayout.Space(2);
            };
        }
    }
}
#endif
