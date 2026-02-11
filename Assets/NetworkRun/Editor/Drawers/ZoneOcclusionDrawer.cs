#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ZoneOcclusionDrawer : ActivityComponentDrawer<ZoneOcclusion>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();
        private ResourceContributionHandlerDrawer _contributionDrawer = new ResourceContributionHandlerDrawer();

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
            var handler = component.GetContributionHandler();
            var isRevealed = component.IsRevealed;
            var useUnitInventory = currentRun?.Settings?.zoneContributionMode == ResourceCollectionMode.UnitInventory;

            _cardDrawer.Draw(
                unit,
                () =>
                {
                    _contributionDrawer.DrawContributeButton(handler, unit, isRevealed, useUnitInventory, onChanged);
                    _contributionDrawer.DrawRemoveButton(activity, unit, currentRun?.Settings, onChanged);
                },
                _contributionDrawer.GetProgressProvider(handler, unit, isRevealed, useUnitInventory),
                () =>
                {
                    if (!isRevealed)
                    {
                        _contributionDrawer.DrawStatusLabel(handler, unit, isRevealed, useUnitInventory);
                    }
                },
                currentRun?.Settings);
        }
    }
}
#endif
