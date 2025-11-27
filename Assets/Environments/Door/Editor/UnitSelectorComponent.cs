using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// Selector component for adding units to a room
/// </summary>
public class UnitSelectorComponent : AssetSelectorComponent<UnitInstance>
{
    public UnitSelectorComponent(string label, Action<UnitInstance> onAssetChanged, bool showHistoryButtons = false)
        : base(label, onAssetChanged, showHistoryButtons)
    {
    }
}
