#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public static class ItemDrawer
    {
        public static void DrawItem(
            Texture icon,
            string displayName,
            string subtitle,
            Color backgroundColor,
            List<UnitDrawerItemAction> shortcuts,
            List<UnitDrawerItemAction> menuItems,
            List<UnitDrawerDisplayItem> displayItems = null)
        {
            EditorGUILayout.BeginHorizontal();
            Color originalBG = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            Color originalColor = GUI.color;
            GUI.color = Color.white;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal();

            // Left side: icon (always takes up space for consistent width)
            EditorGUILayout.BeginVertical(GUILayout.Width(32));
            if (icon != null)
            {
                GUIContent content = new GUIContent(icon);
                GUILayout.Label(content, GUILayout.Width(32), GUILayout.Height(32));
            }
            else
            {
                GUILayout.Space(32);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(32);

            // Right side: name and info
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(subtitle))
            {
                EditorGUILayout.Space(-2);
                GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontStyle = FontStyle.Italic,
                    normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
                };
                EditorGUILayout.LabelField(subtitle, subtitleStyle);
            }

            // Display items
            if (displayItems != null)
            {
                foreach (var displayItem in displayItems)
                {
                    if (displayItem.condition())
                    {
                        GUI.color = displayItem.color;
                        displayItem.drawAction();
                        GUI.color = Color.white;
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // Actions column
            EditorGUILayout.BeginVertical();

            // Shortcuts
            if (shortcuts != null)
            {
                foreach (var actionItem in shortcuts)
                {
                    if (actionItem.condition())
                    {
                        Color originalBGShortcut = GUI.backgroundColor;
                        GUI.backgroundColor = actionItem.backgroundColor ?? originalBGShortcut;
                        if (GUILayout.Button(new GUIContent(actionItem.emoji, actionItem.text), GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            actionItem.action();
                        }
                        GUI.backgroundColor = originalBGShortcut;
                    }
                }
            }

            // Menu button
            if (menuItems != null && menuItems.Count > 0)
            {
                DrawMenuButton(menuItems);
            }
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = originalBG;
            GUI.color = originalColor;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        public static void DrawMenuButton(List<UnitDrawerItemAction> menuItems, float width = 20, float height = 20)
        {
            if (GUILayout.Button("...", GUILayout.Width(width), GUILayout.Height(height)))
            {
                GenericMenu menu = new GenericMenu();
                foreach (var actionItem in menuItems)
                {
                    if (actionItem.condition())
                    {
                        menu.AddItem(new GUIContent(actionItem.text), false, () => actionItem.action());
                    }
                }
                menu.ShowAsContext();
            }
        }
    }
}
#endif
