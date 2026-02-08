using UnityEngine;
using System;

namespace TheFungalNetwork.Editor
{
    public class CardDrawerDisplayItem
    {
        public Func<bool> condition;
        public Color color;
        public Action drawAction;
    }
}
