#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class UnitDropZoneDrawer
    {
        public void Draw(
            string label,
            System.Action<Rect> drawContent,
            System.Func<UnitInstance, bool> canDrop,
            System.Action<UnitInstance> onDrop,
            DragAndDropVisualMode visualMode = DragAndDropVisualMode.Copy)
        {
            var evt = Event.current;

            // Begin the drop zone container
            var dropZoneStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(8, 8, 8, 8)
            };

            EditorGUILayout.BeginVertical(dropZoneStyle);

            // Label at top
            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                normal = { textColor = new Color(0.6f, 0.8f, 1f) }
            };
            EditorGUILayout.LabelField(label, labelStyle, GUILayout.Height(16));

            EditorGUILayout.Space(4);

            // Draw the content inside the drop zone
            var contentRect = EditorGUILayout.BeginVertical();
            drawContent?.Invoke(contentRect);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            // Get the rect after EndVertical
            var dropRect = GUILayoutUtility.GetLastRect();

            // Check if we're hovering during a drag
            bool isHovering = false;
            var draggedUnit = DragAndDrop.GetGenericData("UnitInstance") as UnitInstance;
            if (draggedUnit != null && dropRect.Contains(evt.mousePosition) && (canDrop == null || canDrop(draggedUnit)))
            {
                isHovering = true;
            }

            // Handle drag and drop
            if (evt.type == EventType.DragUpdated && dropRect.Contains(evt.mousePosition))
            {
                if (draggedUnit != null)
                {
                    if (canDrop == null || canDrop(draggedUnit))
                    {
                        DragAndDrop.visualMode = visualMode;
                    }
                    else
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    }
                    evt.Use();
                }
            }

            if (evt.type == EventType.DragPerform && dropRect.Contains(evt.mousePosition))
            {
                if (draggedUnit != null && (canDrop == null || canDrop(draggedUnit)))
                {
                    DragAndDrop.AcceptDrag();
                    onDrop?.Invoke(draggedUnit);
                    evt.Use();
                }
            }

            // Draw hover highlight as border only (not over content)
            if (isHovering && evt.type == EventType.Repaint)
            {
                var highlightColor = visualMode == DragAndDropVisualMode.Move
                    ? new Color(1f, 0.8f, 0.4f, 0.8f)
                    : new Color(0.4f, 0.8f, 1f, 0.8f);

                // Draw border around the drop zone
                var borderWidth = 2f;

                // Top border
                EditorGUI.DrawRect(new Rect(dropRect.x, dropRect.y, dropRect.width, borderWidth), highlightColor);
                // Bottom border
                EditorGUI.DrawRect(new Rect(dropRect.x, dropRect.yMax - borderWidth, dropRect.width, borderWidth), highlightColor);
                // Left border
                EditorGUI.DrawRect(new Rect(dropRect.x, dropRect.y, borderWidth, dropRect.height), highlightColor);
                // Right border
                EditorGUI.DrawRect(new Rect(dropRect.xMax - borderWidth, dropRect.y, borderWidth, dropRect.height), highlightColor);
            }
        }
    }
}
#endif
