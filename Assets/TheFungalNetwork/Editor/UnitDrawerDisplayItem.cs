using UnityEngine;
using System;

namespace TheFungalNetwork.Editor
{
    public abstract class UnitDrawerDisplayItem
    {
        public Func<bool> condition;
        public Color color;
        public Action drawAction;
    }
}
