#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    /// <summary>
    /// Reusable drawer for ResourceContributionHandler UI elements.
    /// Handles all common status labels, buttons, and progress indicators.
    /// </summary>
    public class ResourceContributionHandlerDrawer
    {
        /// <summary>
        /// Draw the status label for a unit
        /// </summary>
        public void DrawStatusLabel(
            ResourceContributionHandler handler,
            UnitInstance unit,
            bool isComplete,
            bool useUnitInventory)
        {
            // Only show status if using unit inventory mode or if complete
            if (!useUnitInventory && !isComplete)
            {
                return;
            }

            var status = handler.GetUnitStatus(unit, isComplete, useUnitInventory);
            var statusText = handler.GetStatusText(status);
            var statusColor = handler.GetStatusColor(status);

            var statusStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 8,
                normal = { textColor = statusColor }
            };

            EditorGUILayout.LabelField(statusText, statusStyle, GUILayout.Height(12), GUILayout.Width(90));
            GUILayout.Space(2);
        }

        /// <summary>
        /// Draw the contribute button for a unit (if applicable)
        /// </summary>
        public bool DrawContributeButton(
            ResourceContributionHandler handler,
            UnitInstance unit,
            bool isComplete,
            bool useUnitInventory,
            System.Action onContribute = null)
        {
            if (!handler.ShouldShowContributeButton(unit, isComplete, useUnitInventory))
            {
                return false;
            }

            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
            var buttonText = $"✓ Contribute {handler.RequiredItem.DisplayName}";

            if (GUILayout.Button(buttonText, GUILayout.Height(20), GUILayout.Width(90)))
            {
                handler.ContributeFromUnit(unit);
                onContribute?.Invoke();
                GUI.backgroundColor = Color.white;
                return true;
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(2);
            return false;
        }

        /// <summary>
        /// Get progress provider function for PartyUnitCardDrawer
        /// </summary>
        public System.Func<float> GetProgressProvider(
            ResourceContributionHandler handler,
            UnitInstance unit,
            bool isComplete,
            bool useUnitInventory)
        {
            if (handler.ShouldShowProgress(unit, isComplete, useUnitInventory))
            {
                return () => handler.GetNormalizedProgress(unit);
            }
            return null;
        }

        /// <summary>
        /// Draw debug remove button
        /// </summary>
        public void DrawRemoveButton(
            ActivityInstance activity,
            UnitInstance unit,
            NetworkRunSettings settings,
            System.Action onRemove = null)
        {
            if (settings?.debugMode != true)
            {
                return;
            }

            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
            if (GUILayout.Button("Remove", GUILayout.Height(18), GUILayout.Width(90)))
            {
                activity.RemoveUnit(unit);
                UnityEditor.AssetDatabase.SaveAssets();
                onRemove?.Invoke();
            }
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Draw summon button for completing a summon action
        /// </summary>
        public bool DrawSummonButton(
            System.Func<bool> canSummon,
            System.Action onSummon,
            int summonedCount,
            string buttonText = "🧙 Summon Unit")
        {
            if (!canSummon())
            {
                return false;
            }

            GUI.backgroundColor = new Color(0.6f, 1f, 0.6f);

            var fullButtonText = $"{buttonText} ({summonedCount} summoned)";
            if (GUILayout.Button(fullButtonText, GUILayout.Height(24)))
            {
                onSummon?.Invoke();
                GUI.backgroundColor = Color.white;
                return true;
            }

            GUI.backgroundColor = Color.white;
            return false;
        }
    }
}
#endif
