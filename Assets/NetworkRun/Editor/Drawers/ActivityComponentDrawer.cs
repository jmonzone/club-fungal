#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace TheFungalNetwork.Editor
{
    public abstract class ActivityComponentDrawer
    {
        public abstract Type ComponentType { get; }
        public abstract void DrawUnitCard(UnitInstance unit, ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged);
        public virtual List<CardDrawerDisplayItem> GetDisplayItems(ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            return null;
        }

        public virtual string GetDropZoneLabel(ActivityInstance activity, ActivityComponent component)
        {
            return null; // Return null to use default label
        }

        public virtual bool ProvidesCustomDropZone(ActivityInstance activity, ActivityComponent component)
        {
            return false; // Return true if component drawer handles its own drop zone
        }

    }

    public abstract class ActivityComponentDrawer<T> : ActivityComponentDrawer where T : ActivityComponent
    {
        public override Type ComponentType => typeof(T);

        public override void DrawUnitCard(UnitInstance unit, ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            if (component is T typedComponent)
            {
                DrawTypedUnitCard(unit, activity, typedComponent, currentRun, onChanged);
            }
        }

        protected abstract void DrawTypedUnitCard(UnitInstance unit, ActivityInstance activity, T component, NetworkRun currentRun, System.Action onChanged);
    }
}
#endif
