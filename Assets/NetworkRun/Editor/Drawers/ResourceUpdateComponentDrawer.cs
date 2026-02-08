#if UNITY_EDITOR
using System.Collections.Generic;

namespace TheFungalNetwork.Editor
{
    public class ResourceUpdateComponentDrawer : ActivityComponentDrawer<ResourceUpdateComponent>
    {
        private ResourceUnitCardDrawer _cardDrawer = new ResourceUnitCardDrawer();

        public override List<CardDrawerDisplayItem> GetDisplayItems(ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            if (component is ResourceUpdateComponent resourceUpdateComponent)
            {
                return new List<CardDrawerDisplayItem>
                {
                    new ResourceProgressDisplayItem(resourceUpdateComponent, activity, currentRun, onChanged)
                };
            }
            return null;
        }

        protected override void DrawTypedUnitCard(UnitInstance unit, ActivityInstance activity, ResourceUpdateComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            _cardDrawer.Draw(unit, activity, component, currentRun, onChanged);
        }
    }
}
#endif
