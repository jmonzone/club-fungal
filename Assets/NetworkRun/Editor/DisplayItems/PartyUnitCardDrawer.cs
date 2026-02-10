#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class PartyUnitCardDrawer
    {

        // Simple draw for party units (no activity)
        public void Draw(UnitInstance unit, NetworkRunSettings settings = null)
        {
            DrawCard(unit, null, null, null, settings);
        }

        // Draw with custom display items (for unlock activities, etc.)
        public void Draw(UnitInstance unit, System.Action extraDisplayItems, NetworkRunSettings settings = null)
        {
            DrawCard(unit, extraDisplayItems, null, null, settings);
        }

        // Draw with custom progress and status (for unlock/other activities)
        public void Draw(
            UnitInstance unit,
            System.Action extraDisplayItems,
            System.Func<float> progressProvider,
            System.Action statusDisplay,
            NetworkRunSettings settings = null)
        {
            DrawCard(unit, extraDisplayItems, progressProvider, statusDisplay, settings);
        }

        private void DrawCard(
            UnitInstance unit,
            System.Action extraDisplayItems,
            System.Func<float> progressProvider,
            System.Action statusDisplay,
            NetworkRunSettings settings)
        {
            var hasExtraDisplayItems = extraDisplayItems != null;
            var hasCustomProgress = progressProvider != null || statusDisplay != null;
            var height = hasExtraDisplayItems || hasCustomProgress ? 145 : 100;

            var cardRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(100), GUILayout.Height(height), GUILayout.ExpandWidth(false));

            // Draw drag handle at top
            DrawDragHandle(unit);

            // Block drop zone from accepting drops on this card
            var evt = Event.current;
            if (cardRect.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                    evt.Use();
                }
                else if (evt.type == EventType.DragPerform)
                {
                    evt.Use();
                }
            }

            // Always draw: icon, name
            DrawIcon(unit);
            DrawName(unit);

            // DrawSpeciesType(unit);

            // DrawUnitInventory(unit);

            if (settings == null || settings.showUnitInventoryButton)
            {
                DrawViewInventoryButton(unit);
            }
            // DrawMaxInventory(unit);

            // Draw extra display items if provided
            if (hasExtraDisplayItems)
            {
                extraDisplayItems?.Invoke();
            }

            // Draw custom progress/status if provided
            if (hasCustomProgress)
            {
                // if (progressProvider != null)
                // {
                //     var progress = progressProvider.Invoke();
                //     DrawProgressBar(progress);
                // }
                if (statusDisplay != null)
                {
                    statusDisplay.Invoke();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawDragHandle(UnitInstance unit)
        {
            var evt = Event.current;

            // Create drag handle area at top of card
            var handleRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(14), GUILayout.Width(90));

            GUILayout.FlexibleSpace();

            var handleStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
                normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
            };

            EditorGUILayout.LabelField("⋮⋮", handleStyle, GUILayout.Width(20), GUILayout.Height(14));

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            // Get actual rect after layout
            var actualRect = GUILayoutUtility.GetLastRect();

            // Enable drag from handle only
            if (evt.type == EventType.MouseDown && actualRect.Contains(evt.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new UnityEngine.Object[] { };
                DragAndDrop.SetGenericData("UnitInstance", unit);
                DragAndDrop.StartDrag(unit.DisplayName);
                evt.Use();
            }

            // Change cursor when hovering over handle
            if (actualRect.Contains(evt.mousePosition))
            {
                EditorGUIUtility.AddCursorRect(actualRect, MouseCursor.MoveArrow);
            }
        }

        private void DrawIcon(UnitInstance unit)
        {
            var icon = unit.Species?.Sprite?.texture;
            if (icon != null)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
                GUILayout.FlexibleSpace();
                GUILayout.Box(icon, GUILayout.Width(40), GUILayout.Height(40));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(40);
            }
        }

        private void DrawName(UnitInstance unit)
        {
            var nameStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, wordWrap = true };
            EditorGUILayout.LabelField(unit.DisplayName, nameStyle, GUILayout.Height(16), GUILayout.Width(90));
        }

        private void DrawSpeciesType(UnitInstance unit)
        {
            if (unit.Species?.Type != null)
            {
                var typeStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 8,
                    normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
                };
                EditorGUILayout.LabelField(unit.Species.Type.Id, typeStyle, GUILayout.Height(12), GUILayout.Width(90));
            }
        }

        private void DrawInventoryItem(ItemTemplate item, int count)
        {
            if (item == null) return;
            var icon = item.Sprite != null ? item.Sprite.texture : null;
            var label = $"{count}x {item.DisplayName ?? item.Id}";
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 8,
                normal = { textColor = count > 0 ? new Color(0.5f, 1f, 0.5f) : new Color(0.7f, 0.7f, 0.7f) }
            };
            EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
            if (icon != null)
                GUILayout.Box(icon, GUILayout.Width(14), GUILayout.Height(14));
            else
                GUILayout.Space(14);
            EditorGUILayout.LabelField(label, style, GUILayout.Height(14), GUILayout.Width(66));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawUnitInventory(UnitInstance unit)
        {

            if (unit?.Inventory?.ItemStacks != null && unit.Inventory.ItemStacks.Count > 0)
            {
                foreach (var itemStack in unit.Inventory.ItemStacks)
                {
                    if (itemStack?.item != null && itemStack.count > 0)
                    {
                        DrawInventoryItem(itemStack.item, itemStack.count);
                    }
                }
            }
            // else
            // {
            //     var emptyStyle = new GUIStyle(EditorStyles.miniLabel)
            //     {
            //         alignment = TextAnchor.MiddleCenter,
            //         fontSize = 8,
            //         fontStyle = FontStyle.Italic,
            //         normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
            //     };
            //     EditorGUILayout.LabelField("empty inventory", emptyStyle, GUILayout.Height(14), GUILayout.Width(90));
            // }
        }

        private void DrawMaxInventory(UnitInstance unit)
        {
            if (unit?.Inventory != null)
            {
                var maxCapacity = unit.Inventory.MaxCapacity;
                var totalCount = unit.Inventory.TotalItemCount;
                var capacityText = maxCapacity > 0 ? $"Inventory {totalCount}/{maxCapacity}" : $"Inventory {totalCount}/∞";
                var capacityStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 7,
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
                };
                EditorGUILayout.LabelField(capacityText, capacityStyle, GUILayout.Height(10), GUILayout.Width(90));
            }
            else
            {
                GUILayout.Space(10);
            }
        }

        private void DrawProgressBar(float progress)
        {
            var unitProgressRect = EditorGUILayout.GetControlRect(false, 8, GUILayout.Width(90));
            EditorGUI.ProgressBar(unitProgressRect, progress, "");
        }

        private void DrawViewInventoryButton(UnitInstance unit)
        {
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 8,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.backgroundColor = new Color(0.85f, 0.85f, 1f);
            if (GUILayout.Button("View Inventory", buttonStyle, GUILayout.Height(16), GUILayout.Width(90)))
            {
                UnitInstanceInspectorWindow.ShowWindow(unit);
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
