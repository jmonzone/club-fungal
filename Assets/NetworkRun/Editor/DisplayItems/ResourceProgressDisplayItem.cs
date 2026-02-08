#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ResourceProgressDisplayItem : UnitDrawerDisplayItem
    {
        private ResourceUnitCardDrawer _unitCardDrawer = new ResourceUnitCardDrawer();
        private UnitDropZoneDrawer _dropZoneDrawer = new UnitDropZoneDrawer();

        public ResourceProgressDisplayItem(ResourceUpdateComponent resourceComponent, ActivityInstance activity, NetworkRun currentRun, System.Action onChanged)
        {
            condition = () => true;
            color = Color.white;
            drawAction = () =>
            {
                EditorGUILayout.Space(4);

                var unitCount = activity.Units?.Count ?? 0;
                var itemName = resourceComponent.ItemTemplate?.DisplayName ?? "Unknown";

                // Draw drop zone with units inside
                DrawDropZoneWithUnits(activity, currentRun, onChanged, resourceComponent, itemName);

                EditorGUILayout.Space(2);
            };
        }

        private void DrawDropZoneWithUnits(ActivityInstance activity, NetworkRun currentRun, System.Action onChanged, ResourceUpdateComponent resourceComponent, string itemName)
        {
            var hasUnits = activity.Units != null && activity.Units.Count > 0;

            _dropZoneDrawer.Draw(
                isEmpty: !hasUnits,
                drawContent: (contentRect) =>
                {
                    // Show only units that are IN this activity
                    var unitCount = activity.Units?.Count ?? 0;
                    EditorGUILayout.BeginHorizontal();
                    int count = 0;
                    const int itemsPerRow = 3;

                    foreach (var unit in activity.Units)
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
                },
                canDrop: (draggedUnit) => !(activity.Units != null && activity.Units.Contains(draggedUnit)),
                onDrop: (draggedUnit) =>
                {
                    // Add unit to activity
                    var allActivities = currentRun?.CurrentRoom?.Data?.activities;
                    activity.AddUnit(draggedUnit, allActivities);

                    UnityEditor.AssetDatabase.SaveAssets();
                    onChanged?.Invoke();
                },
                visualMode: DragAndDropVisualMode.Copy
            );
        }
    }
}
#endif
