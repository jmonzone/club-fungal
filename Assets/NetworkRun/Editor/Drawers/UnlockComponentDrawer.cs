#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class UnlockComponentDrawer : ActivityComponentDrawer<UnlockComponent>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();
        private ResourceContributionHandlerDrawer _contributionDrawer = new ResourceContributionHandlerDrawer();

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
            var handler = component.GetContributionHandler();
            var isUnlocked = component.IsUnlocked;

            _cardDrawer.Draw(
                unit,
                () =>
                {
                    _contributionDrawer.DrawRemoveButton(activity, unit, currentRun?.Settings, onChanged);
                },
                _contributionDrawer.GetProgressProvider(handler, unit, isUnlocked, true),
                () =>
                {
                    _contributionDrawer.DrawStatusLabel(handler, unit, isUnlocked, true);
                },
                currentRun?.Settings);
        }
    }
}
#endif
