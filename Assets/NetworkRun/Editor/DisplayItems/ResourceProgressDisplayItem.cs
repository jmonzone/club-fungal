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
                var collectToGlobal = currentRun?.Settings?.resourceCollectionMode == ResourceCollectionMode.GlobalInventory;

                EditorGUILayout.Space(4);

                // Show resource icon and inventory count only when collecting to global inventory
                if (collectToGlobal && resourceComponent.ItemTemplate?.Sprite != null)
                {
                    EditorGUILayout.BeginHorizontal();

                    var icon = resourceComponent.ItemTemplate.Sprite.texture;
                    GUILayout.Box(icon, GUILayout.Width(32), GUILayout.Height(32));

                    GUILayout.Space(8);

                    EditorGUILayout.BeginVertical();

                    // Calculate total resources collected in global inventory
                    int resourceCount = 0;

                    if (currentRun?.Inventory != null)
                    {
                        resourceCount = currentRun.Inventory.GetItemCount(resourceComponent.ItemTemplate);
                    }

                    var countStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 14,
                        normal = { textColor = new Color(0.3f, 0.8f, 0.3f) }
                    };
                    EditorGUILayout.LabelField($"{resourceCount}x {resourceComponent.ItemTemplate.DisplayName}", countStyle);

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(4);
                }

                var unitCount = activity.Units?.Count ?? 0;
                var itemName = resourceComponent.ItemTemplate?.DisplayName ?? "Unknown";

                // Show what's being collected with type bonuses considered (debug mode only)
                if (currentRun?.Settings?.debugMode == true)
                {
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
                }

                // Show total collected
                if (collectToGlobal && currentRun?.Inventory != null)
                {
                    // var globalCount = currentRun.Inventory.GetItemCount(resourceComponent.ItemTemplate);
                    // var countStyle = new GUIStyle(EditorStyles.miniLabel)
                    // {
                    //     fontStyle = FontStyle.Bold,
                    //     normal = { textColor = new Color(0.3f, 1f, 0.3f) }
                    // };
                    // EditorGUILayout.LabelField($"📦 Total collected: {globalCount}x {itemName}", countStyle);
                }
                else if (!collectToGlobal && activity.Units != null && currentRun?.Settings?.debugMode == true)
                {
                    // Show total across all units in activity (debug mode only)
                    int totalInUnits = 0;
                    foreach (var unit in activity.Units)
                    {
                        if (unit?.Inventory != null)
                        {
                            totalInUnits += unit.Inventory.GetItemCount(resourceComponent.ItemTemplate);
                        }
                    }
                    if (totalInUnits > 0)
                    {
                        var countStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            fontStyle = FontStyle.Bold,
                            normal = { textColor = new Color(0.3f, 1f, 0.3f) }
                        };
                        EditorGUILayout.LabelField($"📦 In units: {totalInUnits}x {itemName}", countStyle);
                    }
                }

                EditorGUILayout.Space(2);
            };
        }
    }
}
#endif
