using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Base class for asset selectors with undo/redo functionality
/// </summary>
public class AssetSelectorComponent<T> where T : UnityEngine.Object
{
    private T currentAsset;
    private List<T> assetHistory = new List<T>();
    private int currentHistoryIndex = -1;
    private string label;
    private Action<T> onAssetChanged;
    private bool showHistoryButtons;

    public T CurrentAsset => currentAsset;

    public AssetSelectorComponent(string label, Action<T> onAssetChanged, bool showHistoryButtons = false)
    {
        this.label = label;
        this.onAssetChanged = onAssetChanged;
        this.showHistoryButtons = showHistoryButtons;
    }

    public void Initialize(T initialAsset)
    {
        // Only initialize if we haven't started tracking history yet
        if (assetHistory.Count == 0 && initialAsset != null)
        {
            currentAsset = initialAsset;
            assetHistory.Add(initialAsset);
            currentHistoryIndex = 0;
        }
    }

    public void Reset()
    {
        currentAsset = null;
        assetHistory = new List<T>();
        currentHistoryIndex = -1;
    }

    public void DrawGUI(bool? overrideShowHistoryButtons = null)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        currentAsset = (T)EditorGUILayout.ObjectField(label, currentAsset, typeof(T), false);
        if (EditorGUI.EndChangeCheck() && currentAsset != null)
        {
            // Remove any forward history when a new asset is applied
            if (currentHistoryIndex < assetHistory.Count - 1)
            {
                assetHistory.RemoveRange(currentHistoryIndex + 1, assetHistory.Count - currentHistoryIndex - 1);
            }

            assetHistory.Add(currentAsset);
            currentHistoryIndex = assetHistory.Count - 1;
            onAssetChanged?.Invoke(currentAsset);

            Debug.Log($"Asset changed to: {currentAsset?.name}. Asset History count: {assetHistory.Count}History count: {assetHistory.Count}, Current index: {currentHistoryIndex}");
        }
        EditorGUILayout.EndHorizontal();

        bool shouldShowButtons = overrideShowHistoryButtons ?? showHistoryButtons;
        if (shouldShowButtons)
        {
            DrawHistoryButtons();
        }
    }

    private void DrawHistoryButtons()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // Undo button - disabled if at the beginning of history or no history
        EditorGUI.BeginDisabledGroup(currentHistoryIndex <= 0 || assetHistory.Count <= 1);
        if (GUILayout.Button("↶", GUILayout.Width(30), GUILayout.Height(30)))
        {
            currentHistoryIndex--;
            currentAsset = assetHistory[currentHistoryIndex];
            onAssetChanged?.Invoke(currentAsset);
            Debug.Log($"Undo: Current index: {currentHistoryIndex}, Asset: {currentAsset.name}");
        }
        EditorGUI.EndDisabledGroup();

        // Redo button - disabled if at the end of history
        EditorGUI.BeginDisabledGroup(currentHistoryIndex >= assetHistory.Count - 1 || assetHistory.Count == 0);
        if (GUILayout.Button("↷", GUILayout.Width(30), GUILayout.Height(30)))
        {
            currentHistoryIndex++;
            currentAsset = assetHistory[currentHistoryIndex];
            onAssetChanged?.Invoke(currentAsset);
            Debug.Log($"Redo: Current index: {currentHistoryIndex}, Asset: {currentAsset.name}");
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }
}
