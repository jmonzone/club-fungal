#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class UnlockComponentDrawer : ActivityComponentDrawer<UnlockComponent>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();

        public override List<CardDrawerDisplayItem> GetDisplayItems(ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            if (component is UnlockComponent unlockComponent && currentRun?.CurrentRoom?.Data?.doors != null)
            {
                return new List<CardDrawerDisplayItem>
                {
                    new UnlockInfoDisplayItem(activity, currentRun, unlockComponent, onChanged)
                };
            }
            return null;
        }

        protected override void DrawTypedUnitCard(UnitInstance unit, ActivityInstance activity, UnlockComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            var resourceItem = component.ResourceCondition?.RequiredItem;
            var hasResource = resourceItem != null && unit.Inventory.GetItemCount(resourceItem) > 0;
            var isUnlocked = component.IsUnlocked;

            _cardDrawer.Draw(
                unit,
                () =>
                {
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
                (hasResource && !isUnlocked) ? () => component.GetUnitProgress(unit) / component.UpdateInterval : null,
                () =>
                {
                    // Status with resource info
                    if (isUnlocked)
                    {
                        var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 8,
                            normal = { textColor = new Color(1f, 0.85f, 0.3f) }
                        };
                        EditorGUILayout.LabelField("✓ Task Complete", statusStyle, GUILayout.Height(12), GUILayout.Width(90));
                    }
                    else if (hasResource)
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
                },
                currentRun?.Settings);
        }
    }
}
#endif
