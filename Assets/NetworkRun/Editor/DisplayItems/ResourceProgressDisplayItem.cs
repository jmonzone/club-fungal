#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ResourceProgressDisplayItem : UnitDrawerDisplayItem
    {
        private ResourceUnitCardDrawer _unitCardDrawer = new ResourceUnitCardDrawer();

        public ResourceProgressDisplayItem(ResourceUpdateComponent resourceComponent, ActivityInstance activity, NetworkRun currentRun, System.Action onChanged)
        {
            condition = () => true;
            color = Color.white;
            drawAction = () =>
            {
                EditorGUILayout.Space(4);

                var unitCount = activity.Units?.Count ?? 0;
                var itemName = resourceComponent.ItemTemplate?.DisplayName ?? "Unknown";

                // Show all party units in a grid
                if (currentRun?.Party != null && currentRun.Party.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    int count = 0;
                    const int itemsPerRow = 3;

                    foreach (var unit in currentRun.Party)
                    {
                        if (unit == null) continue;

                        if (count > 0 && count % itemsPerRow == 0)
                        {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space(2);
                            EditorGUILayout.BeginHorizontal();
                        }

                        _unitCardDrawer.Draw(unit, activity, resourceComponent, currentRun, onChanged);

                        count++;
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);

                    // Show what's being collected with type bonuses considered
                    var totalItems = resourceComponent.ItemsPerUpdate * unitCount;

                    if (unitCount > 0)
                    {
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
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"⏱ Each cycle: 0x {itemName} ({resourceComponent.UpdateInterval:F1}s)", EditorStyles.miniLabel);
                    }
                }

                EditorGUILayout.Space(2);
            };
        }
    }
}
#endif
