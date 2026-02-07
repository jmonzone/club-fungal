#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class PartyUnitsGridDisplayItem : UnitDrawerDisplayItem
    {
        public PartyUnitsGridDisplayItem(ActivityInstance activity, RoomTemplate selectedRoom, List<UnitInstance> party, System.Action onChanged, NetworkRun currentRun = null, System.Action<ActivityInstance, UnitInstance> onAddUnit = null, System.Action<ActivityInstance, UnitInstance> onRemoveUnit = null)
        {
            condition = () => true;
            color = new Color(0.95f, 0.95f, 1f);
            drawAction = () =>
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Party:", EditorStyles.miniLabel);

                EditorGUILayout.BeginHorizontal();
                int count = 0;
                const int itemsPerRow = 6;

                foreach (var unit in party)
                {
                    if (unit == null) continue;

                    if (count > 0 && count % itemsPerRow == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    var isInActivity = activity.Units != null && activity.Units.Contains(unit);
                    var buttonColor = isInActivity ? Color.green : GUI.backgroundColor;

                    GUI.backgroundColor = buttonColor;
                    var icon = unit.Species?.Sprite?.texture;
                    var content = icon != null
                        ? new GUIContent(icon, unit.DisplayName)
                        : new GUIContent(unit.DisplayName.Substring(0, System.Math.Min(2, unit.DisplayName.Length)));

                    if (GUILayout.Button(content, GUILayout.Width(40), GUILayout.Height(40)))
                    {
                        if (isInActivity)
                        {
                            onRemoveUnit?.Invoke(activity, unit);
                            onChanged?.Invoke();
                        }
                        else
                        {
                            onAddUnit?.Invoke(activity, unit);
                            onChanged?.Invoke();
                        }
                    }
                    GUI.backgroundColor = Color.white;

                    count++;
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
            };
        }
    }
}
#endif
