#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class SummonUnitComponentDrawer : ActivityComponentDrawer<SummonUnitComponent>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();

        public override List<CardDrawerDisplayItem> GetDisplayItems(ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            if (component is SummonUnitComponent summonComponent)
            {
                return new List<CardDrawerDisplayItem>
                {
                    new SummonUnitInfoDisplayItem(activity, currentRun, summonComponent, onChanged)
                };
            }
            return null;
        }

        protected override void DrawTypedUnitCard(UnitInstance unit, ActivityInstance activity, SummonUnitComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            var resourceItem = component.RequiredItem;
            var hasResource = resourceItem != null && unit.Inventory.GetItemCount(resourceItem) > 0;
            var useUnitInventory = currentRun?.Settings?.zoneContributionMode == ResourceCollectionMode.UnitInventory;

            // Check if requirements are satisfied
            var primarySatisfied = component.CurrentResourceCount >= component.RequiredAmount;
            var additionalSatisfied = component.AdditionalResourceCost == null ||
                                     component.AdditionalResourceCount >= component.AdditionalResourceCost.Amount;
            var requirementsMet = primarySatisfied && additionalSatisfied;

            _cardDrawer.Draw(
                unit,
                () =>
                {
                    // Show contribute button in unit inventory mode (only if requirements not yet met)
                    if (useUnitInventory && hasResource && !requirementsMet)
                    {
                        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                        var buttonText = $"✓ Contribute {resourceItem.DisplayName}";
                        if (GUILayout.Button(buttonText, GUILayout.Height(20), GUILayout.Width(90)))
                        {
                            component.ContributeFromUnit(unit);
                            onChanged?.Invoke();
                        }
                        GUI.backgroundColor = Color.white;
                        GUILayout.Space(2);
                    }

                    // Remove button (debug mode)
                    if (currentRun.Settings.debugMode)
                    {
                        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                        if (GUILayout.Button("Remove", GUILayout.Height(18), GUILayout.Width(90)))
                        {
                            activity.RemoveUnit(unit);
                            UnityEditor.AssetDatabase.SaveAssets();
                            onChanged?.Invoke();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                },
                (hasResource && useUnitInventory && !requirementsMet) ? () => component.GetUnitProgress(unit) / component.UpdateInterval : null,
                () =>
                {
                    // Only show status if using unit inventory mode
                    if (useUnitInventory)
                    {
                        if (requirementsMet)
                        {
                            // Show ready status when requirements met
                            var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                            {
                                alignment = TextAnchor.MiddleCenter,
                                fontSize = 8,
                                normal = { textColor = new Color(0.6f, 1f, 0.6f) },
                                fontStyle = FontStyle.Bold
                            };
                            EditorGUILayout.LabelField("✓ Ready to summon", statusStyle);
                        }
                        else if (hasResource)
                        {
                            // Show contributing status
                            var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                            {
                                alignment = TextAnchor.MiddleCenter,
                                fontSize = 8,
                                normal = { textColor = new Color(1f, 1f, 0.7f) }
                            };
                            EditorGUILayout.LabelField($"Contributing {resourceItem.DisplayName}...", statusStyle);
                        }
                        else
                        {
                            // Show waiting status
                            var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                            {
                                alignment = TextAnchor.MiddleCenter,
                                fontSize = 8,
                                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
                            };
                            EditorGUILayout.LabelField($"Needs {resourceItem?.DisplayName ?? "resources"}", statusStyle);
                        }
                    }
                }
            );
        }
    }

    public class SummonUnitInfoDisplayItem : CardDrawerDisplayItem
    {
        public SummonUnitInfoDisplayItem(ActivityInstance activity, NetworkRun currentRun, SummonUnitComponent component, System.Action onChanged)
        {
            condition = () => true;
            color = new Color(0.95f, 1f, 0.95f);
            drawAction = () =>
            {
                var primaryItem = component.RequiredItem;
                var primaryCurrent = component.CurrentResourceCount;
                var primaryRequired = component.RequiredAmount;
                var primaryProgress = primaryRequired > 0 ? (float)primaryCurrent / primaryRequired : 0f;

                var additionalCost = component.AdditionalResourceCost;
                var additionalCurrent = component.AdditionalResourceCount;
                var additionalRequired = additionalCost?.Amount ?? 0;
                var additionalProgress = additionalRequired > 0 ? (float)additionalCurrent / additionalRequired : 0f;

                var primarySatisfied = primaryCurrent >= primaryRequired;
                var additionalSatisfied = additionalCost == null || additionalCurrent >= additionalRequired;
                var requirementsMet = primarySatisfied && additionalSatisfied;
                var canSummon = component.CanSummon();

                var useGlobalInventory = currentRun?.Settings?.zoneContributionMode == ResourceCollectionMode.GlobalInventory;

                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Primary resource progress
                EditorGUILayout.BeginHorizontal();

                if (primaryItem?.Sprite != null)
                {
                    var icon = primaryItem.Sprite.texture;
                    GUILayout.Box(icon, GUILayout.Width(20), GUILayout.Height(20));
                }
                else
                {
                    GUILayout.Space(20);
                }

                var rect = EditorGUILayout.GetControlRect(false, 20);
                var resourceName = primaryItem?.DisplayName ?? "Primary Resource";
                EditorGUI.ProgressBar(rect, primaryProgress, $"{resourceName}: {primaryCurrent}/{primaryRequired}");
                EditorGUILayout.EndHorizontal();

                // Additional resource progress (if present)
                if (additionalCost != null && additionalCost.Item != null)
                {
                    EditorGUILayout.Space(2);
                    EditorGUILayout.BeginHorizontal();

                    if (additionalCost.Item.Sprite != null)
                    {
                        var icon = additionalCost.Item.Sprite.texture;
                        GUILayout.Box(icon, GUILayout.Width(20), GUILayout.Height(20));
                    }
                    else
                    {
                        GUILayout.Space(20);
                    }

                    var additionalRect = EditorGUILayout.GetControlRect(false, 20);
                    var additionalName = additionalCost.Item?.DisplayName ?? "Additional Resource";
                    EditorGUI.ProgressBar(additionalRect, additionalProgress, $"{additionalName}: {additionalCurrent}/{additionalRequired}");
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(4);

                // Status display
                if (requirementsMet && canSummon)
                {
                    var statusStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = new Color(0.6f, 1f, 0.6f) }
                    };
                    EditorGUILayout.LabelField($"✓ Ready to summon! ({component.SummonedCount} summoned)", statusStyle);
                }
                else if (!canSummon && !requirementsMet)
                {
                    var statusStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = new Color(1f, 0.6f, 0.6f) }
                    };
                    EditorGUILayout.LabelField("Max summons reached", statusStyle);
                }

                // Manual summon button (global inventory mode)
                if (useGlobalInventory && canSummon && requirementsMet)
                {
                    GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
                    if (GUILayout.Button("🧙 Summon Unit", GUILayout.Height(24)))
                    {
                        var newUnit = component.SummonUnit(currentRun);
                        if (newUnit != null)
                        {
                            Debug.Log($"Summoned new unit: {newUnit.DisplayName}!");
                        }
                        onChanged?.Invoke();
                    }
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            };
        }
    }
}
#endif
