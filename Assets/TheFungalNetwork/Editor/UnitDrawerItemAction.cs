using UnityEngine;
using System;

namespace TheFungalNetwork.Editor
{
    public abstract class UnitDrawerItemAction
    {
        /// <summary>
        /// The display text for the action.
        /// </summary>
        public string text;

        /// <summary>
        /// The emoji to display for the action.
        /// </summary>
        public string emoji;

        /// <summary>
        /// The action to perform when the item is selected.
        /// </summary>
        public Action action;

        /// <summary>
        /// A function to determine if the item should be displayed.
        /// </summary>
        public Func<bool> condition;

        /// <summary>
        /// The background color for the item.
        /// </summary>
        public Color? backgroundColor;
    }
}
