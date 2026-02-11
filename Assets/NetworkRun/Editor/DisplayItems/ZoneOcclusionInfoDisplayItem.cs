#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class ZoneOcclusionInfoDisplayItem : CardDrawerDisplayItem
    {
        public ZoneOcclusionInfoDisplayItem(ActivityInstance activity, NetworkRun currentRun, ZoneOcclusion zoneOcclusion, System.Action onChanged)
        {
            condition = () => true;
            color = new Color(0.95f, 0.95f, 1f);
            drawAction = () =>
            {
                // Don't show anything if zone is already revealed
                if (zoneOcclusion != null && zoneOcclusion.IsRevealed)
                {
                    return;
                }

                EditorGUILayout.Space(4);

                if (zoneOcclusion != null && zoneOcclusion.RequiredItem != null)
                {
                    var primaryEnough = zoneOcclusion.CurrentResourceCount >= zoneOcclusion.RequiredAmount;
                    var additionalEnough = zoneOcclusion.AdditionalResourceCost == null ||
                                           zoneOcclusion.AdditionalResourceCount >= zoneOcclusion.AdditionalResourceCost.Amount;
                    var hasEnough = primaryEnough && additionalEnough;

                    // Show primary resource progress (spores)
                    EditorGUILayout.BeginHorizontal();

                    var resourceItem = zoneOcclusion.RequiredItem;
                    if (resourceItem?.Sprite != null)
                    {
                        var icon = resourceItem.Sprite.texture;
                        GUILayout.Box(icon, GUILayout.Width(20), GUILayout.Height(20));
                    }
                    else
                    {
                        GUILayout.Space(20);
                    }

                    var progress = zoneOcclusion.RequiredAmount > 0
                        ? Mathf.Clamp01((float)zoneOcclusion.CurrentResourceCount / zoneOcclusion.RequiredAmount)
                        : 0f;
                    var rect = EditorGUILayout.GetControlRect(false, 20);
                    var resourceName = resourceItem?.DisplayName ?? "Unknown";
                    EditorGUI.ProgressBar(rect, progress, $"{resourceName}: {zoneOcclusion.CurrentResourceCount}/{zoneOcclusion.RequiredAmount}");

                    EditorGUILayout.EndHorizontal();

                    // Show additional resource progress (if any)
                    if (zoneOcclusion.AdditionalResourceCost != null && zoneOcclusion.AdditionalResourceCost.Item != null)
                    {
                        EditorGUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();

                        var additionalItem = zoneOcclusion.AdditionalResourceCost.Item;
                        if (additionalItem?.Sprite != null)
                        {
                            var icon = additionalItem.Sprite.texture;
                            GUILayout.Box(icon, GUILayout.Width(20), GUILayout.Height(20));
                        }
                        else
                        {
                            GUILayout.Space(20);
                        }

                        var additionalProgress = zoneOcclusion.AdditionalResourceCost.Amount > 0
                            ? Mathf.Clamp01((float)zoneOcclusion.AdditionalResourceCount / zoneOcclusion.AdditionalResourceCost.Amount)
                            : 0f;
                        var additionalRect = EditorGUILayout.GetControlRect(false, 20);
                        var additionalName = additionalItem?.DisplayName ?? "Unknown";
                        EditorGUI.ProgressBar(additionalRect, additionalProgress, $"{additionalName}: {zoneOcclusion.AdditionalResourceCount}/{zoneOcclusion.AdditionalResourceCost.Amount}");

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space(4);

                    // Show status text
                    if (hasEnough)
                    {
                        // Show Reveal Zone button if enough resources collected
                        EditorGUILayout.Space(4);
                        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
                        var buttonText = $"🔮 Reveal Zone";
                        if (GUILayout.Button(buttonText, GUILayout.Height(30)))
                        {
                            zoneOcclusion.RevealZone();
                            onChanged?.Invoke();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                    else
                    {
                        var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontSize = 9,
                            fontStyle = FontStyle.Italic,
                            normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
                        };
                        EditorGUILayout.LabelField("Zone hidden...", statusStyle);
                    }

                    EditorGUILayout.Space(2);
                }
            };
        }
    }
}
#endif
