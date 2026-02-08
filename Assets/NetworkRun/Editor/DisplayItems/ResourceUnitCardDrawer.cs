#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ResourceUnitCardDrawer
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();

        public void Draw(
            UnitInstance unit,
            ActivityInstance activity,
            ResourceUpdateComponent resourceComponent,
            NetworkRun currentRun,
            System.Action onChanged)
        {
            _cardDrawer.Draw(unit, activity, resourceComponent, currentRun, onChanged);
        }
    }
}
#endif
