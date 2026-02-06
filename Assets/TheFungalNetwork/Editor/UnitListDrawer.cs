using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace TheFungalNetwork.Editor
{
    public class UnitListDrawer
    {
        public static void DrawList(IEnumerable<UnitController> controllers, UnitControllerService unitControllerService)
        {
            DrawList(controllers.Select(controller => controller.Instance), unitControllerService);
        }

        public static void DrawList(IEnumerable<UnitInstance> instances, UnitControllerService unitControllerService)
        {
            instances = instances
            .Where(instance => instance != null)
            .OrderBy(i => i.Id);

            // Load services using GURUStyler - consistent with GURUWindow approach
            var unitInstanceService = GURUStyler.LoadAsset<UnitInstanceService>(nameof(UnitInstanceService));
            var partyInstanceService = GURUStyler.LoadAsset<PartyInstanceService>(nameof(PartyInstanceService));

            unitControllerService.Controllers.RemoveAll(c => c == null || c.gameObject == null);

            // Predefine styles outside the loop
            GUIStyle jobStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Italic, normal = { textColor = new Color(0.5f, 0.5f, 0.5f) } };

            foreach (var unitInstance in instances)
            {
                try
                {
                    // Compute instance-specific data
                    var icon = unitInstance?.Species?.Sprite?.texture;
                    var displayName = unitInstance?.DisplayName ?? "No Instance";
                    var job = unitInstance?.Job?.Id.ToUpper() ?? "No Job";
                    var isInParty = partyInstanceService?.PartyInstances?.Any(p => p.Id == unitInstance.Id) ?? false;

                    var initialUnit = unitInstanceService?.InitialUnits.Find(instance => instance.Data.id == unitInstance.Id);

                    // Find controller by comparing IDs directly (string comparison)
                    // Filter out destroyed controllers and match by instance ID
                    var controller = unitControllerService?.Controllers.Find(c => c.Instance != null && c.Instance.Id == unitInstance.Id);

                    bool isSelected = controller != null && Selection.activeGameObject == controller.gameObject;
                    bool isInitial = initialUnit != null;

                    var backgroundColor = (isSelected, isInitial) switch
                    {
                        (isSelected: true, _) => Color.cyan,
                        (isSelected: false, isInitial: true) => new Color(0.5f, 0.6f, 1f),
                        _ => Color.white
                    };

                    var behaviour = isSelected ? (controller?.CurrentBehaviour?.GetType().Name ?? "None") : null;
                    var interaction = isSelected ? (controller?.CurrentInteraction?.name ?? "None") : null;
                    var activityUnit = controller != null ? controller.GetComponent<ActivityUnit>() : null;
                    var activity = activityUnit?.Activity;

                    // Create action lists
                    var menuItems = new List<UnitDrawerItemAction>
                    {
                        new ViewAssetAction(unitInstance.Template, initialUnit != null),
                        new ViewInstanceAction(unitInstance),
                        new TogglePartyAction(isInParty, partyInstanceService, unitInstance),
                        new InviteFriendAction(unitControllerService, controller),
                        new DeleteInstanceAction(unitInstance, controller, initialUnit != null)
                    };

                    var shortcuts = new List<UnitDrawerItemAction>
                    {
                        new CreateControllerAction(unitInstance, unitControllerService),
                        new ViewGameObjectAction(controller),
                    };

                    var displayItems = new List<UnitDrawerDisplayItem>
                    {
                        new InPartyDisplay(isInParty, jobStyle),
                        new BehaviourDisplay(behaviour, jobStyle),
                        new InteractionDisplay(interaction, controller, jobStyle),
                        new ActivityDisplay(activity, jobStyle)
                    };

                    // Draw the unit
                    ItemDrawer.DrawItem(icon, displayName, job, backgroundColor, shortcuts, menuItems, displayItems);
                }
                catch (Exception ex)
                {
                    EditorGUILayout.HelpBox($"Error drawing unit {unitInstance?.Id ?? "unknown"}: {ex.Message}\n{ex.StackTrace}", MessageType.Error);
                }
            }
        }
    }
}
