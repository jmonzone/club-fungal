using UnityEngine;
using System;

/// <summary>
/// Reusable component for selecting textures with undo/redo functionality
/// </summary>
public class TextureSelectorComponent : AssetSelectorComponent<Texture>
{
    public Texture CurrentTexture => CurrentAsset;

    public TextureSelectorComponent(string label, Action<Texture> onTextureChanged)
        : base(label, onTextureChanged)
    {
    }
}
