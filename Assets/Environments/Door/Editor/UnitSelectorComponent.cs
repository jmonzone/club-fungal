using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// Selector component for adding units to a room
/// </summary>
public class UnitSelectorComponent : AssetSelectorComponent<UnitTemplate>
{
    public UnitSelectorComponent(string label, Action<UnitTemplate> onAssetChanged, bool showHistoryButtons = false)
        : base(label, onAssetChanged, showHistoryButtons)
    {
    }
}
