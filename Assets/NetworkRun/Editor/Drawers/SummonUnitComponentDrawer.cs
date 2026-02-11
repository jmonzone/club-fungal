#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class SummonUnitComponentDrawer : ActivityComponentDrawer<SummonUnitComponent>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();
        private ResourceContributionHandlerDrawer _contributionDrawer = new ResourceContributionHandlerDrawer();

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
            var handler = component.GetContributionHandler();
            var useUnitInventory = currentRun?.Settings?.zoneContributionMode == ResourceCollectionMode.UnitInventory;

            _cardDrawer.Draw(
                unit,
                () =>
                {
                    _contributionDrawer.DrawContributeButton(handler, unit, false, useUnitInventory, onChanged);
                    _contributionDrawer.DrawRemoveButton(activity, unit, currentRun?.Settings, onChanged);
                },
                _contributionDrawer.GetProgressProvider(handler, unit, false, useUnitInventory),
                () =>
                {
                    _contributionDrawer.DrawStatusLabel(handler, unit, false, useUnitInventory);
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
                    EditorGUILayout.LabelField("✓ Ready to summon!", statusStyle);
                }
                else if (!canSummon && requirementsMet)
                {
                    var statusStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = new Color(1f, 0.6f, 0.6f) }
                    };
                    EditorGUILayout.LabelField($"Max summons reached ({component.SummonedCount}/{component.MaxSummons})", statusStyle);
                }
                else if (!requirementsMet)
                {
                    var statusStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
                    };
                    EditorGUILayout.LabelField($"Collecting resources... ({component.SummonedCount} summoned)", statusStyle);
                }

                EditorGUILayout.Space(4);

                // Manual summon button (works in both modes)
                if (canSummon && requirementsMet)
                {
                    GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);
                    if (GUILayout.Button("🧙 Summon Unit", GUILayout.Height(28)))
                    {
                        var newUnit = component.SummonUnit(currentRun, activity);
                        if (newUnit != null)
                        {
                            Debug.Log($"[SummonUnit] Summoned {newUnit.DisplayName} to party! Total summoned: {component.SummonedCount}");
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
