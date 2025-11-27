using UnityEngine;
using System;

/// <summary>
/// Reusable component for selecting materials with undo/redo functionality
/// </summary>
public class MaterialSelectorComponent : AssetSelectorComponent<Material>
{
    public Material CurrentMaterial => CurrentAsset;

    public MaterialSelectorComponent(string label, Action<Material> onMaterialChanged)
        : base(label, onMaterialChanged)
    {
    }
}
