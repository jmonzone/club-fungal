using System;

namespace TheFungalNetwork.Editor
{
    public class ActivityItemAction : UnitDrawerItemAction
    {
        public ActivityItemAction(string text, string emoji, Action action, Func<bool> condition = null)
        {
            this.text = text;
            this.emoji = emoji;
            this.action = action;
            this.condition = condition ?? (() => true);
        }
    }
}
