#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ZoneOcclusionDrawer : ActivityComponentDrawer<ZoneOcclusion>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();

        public override List<CardDrawerDisplayItem> GetDisplayItems(ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            if (component is ZoneOcclusion zoneOcclusion)
            {
                return new List<CardDrawerDisplayItem>
                {
                    new ZoneOcclusionInfoDisplayItem(activity, currentRun, zoneOcclusion, onChanged)
                };
            }
            return null;
        }

        protected override void DrawTypedUnitCard(UnitInstance unit, ActivityInstance activity, ZoneOcclusion component, NetworkRun currentRun, System.Action onChanged)
        {
            var resourceItem = component.RequiredItem;
            var hasResource = resourceItem != null && unit.Inventory.GetItemCount(resourceItem) > 0;
            var isRevealed = component.IsRevealed;
            var useUnitInventory = currentRun?.Settings?.zoneContributionMode == ResourceCollectionMode.UnitInventory;

            _cardDrawer.Draw(
                unit,
                () =>
                {
                    // Show contribute button in unit inventory mode
                    if (!isRevealed && useUnitInventory && hasResource)
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
                (hasResource && !isRevealed && useUnitInventory) ? () => component.GetUnitProgress(unit) / component.UpdateInterval : null,
                () =>
                {
                    // Only show status if zone is not revealed and using unit inventory mode
                    if (!isRevealed && useUnitInventory)
                    {
                        if (hasResource)
                        {
                            var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                            {
                                alignment = TextAnchor.MiddleCenter,
                                fontSize = 8,
                                normal = { textColor = new Color(0.7f, 1f, 0.7f) }
                            };
                            EditorGUILayout.LabelField("Contributing...", statusStyle, GUILayout.Height(12), GUILayout.Width(90));
                        }
                        else
                        {
                            var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                            {
                                alignment = TextAnchor.MiddleCenter,
                                fontSize = 8,
                                normal = { textColor = new Color(1f, 0.5f, 0.5f) }
                            };
                            var resourceName = resourceItem?.DisplayName ?? "resource";
                            EditorGUILayout.LabelField($"Needs {resourceName}", statusStyle, GUILayout.Height(12), GUILayout.Width(90));
                        }

                        GUILayout.Space(2);
                    }
                },
                currentRun?.Settings);
        }
    }
}
#endif
