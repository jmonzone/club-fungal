#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheFungalNetwork.Editor
{
    public class DefaultComponentDrawer : ActivityComponentDrawer<ActivityComponent>
    {
        private PartyUnitCardDrawer _cardDrawer = new PartyUnitCardDrawer();

        public override void DrawUnitCard(UnitInstance unit, ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            // Default drawer works even without a component
            DrawTypedUnitCard(unit, activity, component, currentRun, onChanged);
        }

        protected override void DrawTypedUnitCard(UnitInstance unit, ActivityInstance activity, ActivityComponent component, NetworkRun currentRun, System.Action onChanged)
        {
            _cardDrawer.Draw(
                unit,
                () =>
                {
                    // Remove button
                    if (currentRun?.Settings?.debugMode ?? false)
                    {
                        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                        if (GUILayout.Button("Remove", GUILayout.Height(18), GUILayout.Width(90)))
                        {
                            activity.RemoveUnit(unit);
                            UnityEditor.AssetDatabase.SaveAssets();
                            onChanged?.Invoke();
                        }
                        GUI.backgroundColor = Color.white;
                    }
                },
                null,
                null);
        }
    }
}
#endif
