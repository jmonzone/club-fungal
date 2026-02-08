#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class InspectComponentDrawer : ActivityComponentDrawer<InspectComponent>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();

        public override List<CardDrawerDisplayItem> GetDisplayItems(ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            if (component is InspectComponent inspectComponent && currentRun?.CurrentRoom?.Data?.doors != null)
            {
                return new List<CardDrawerDisplayItem>
                {
                    new DoorInfoDisplayItem(currentRun, inspectComponent, onChanged, activity)
                };
            }
            return null;
        }

        protected override void DrawTypedUnitCard(UnitInstance unit, ActivityInstance activity, InspectComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            _cardDrawer.Draw(
                unit,
                () =>
                {
                    // Remove button (debug mode)
                    if (currentRun?.Settings?.debugMode ?? false)
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
                null,
                null);
        }
    }
}
#endif
