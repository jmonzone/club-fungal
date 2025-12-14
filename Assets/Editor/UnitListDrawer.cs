using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Linq;

public abstract class UnitDrawerItemAction
{
    public string text;
    public string emoji;
    public Action action;
    public Func<bool> condition;
    public Color? backgroundColor;
}

public class ViewAssetAction : UnitDrawerItemAction
{
    public ViewAssetAction(UnitInstance unitInstance, bool isInitial)
    {
        text = "View Asset";
        emoji = "📁";
        action = () => { Selection.activeObject = unitInstance; EditorGUIUtility.PingObject(unitInstance); };
        condition = () => isInitial;
    }
}

public class ViewInstanceAction : UnitDrawerItemAction
{
    public ViewInstanceAction(UnitInstance unitInstance)
    {
        text = "View Instance";
        emoji = "👁️";
        action = () => PopupInspector.Show(unitInstance);
        condition = () => true;
    }
}

public class ViewGameObjectAction : UnitDrawerItemAction
{
    public ViewGameObjectAction(UnitController controller)
    {
        text = "View GameObject";
        emoji = "👁️";
        action = () =>
        {
            Selection.objects = new UnityEngine.Object[] { controller.gameObject };
            EditorGUIUtility.PingObject(controller.gameObject);
            var sceneView = SceneView.lastActiveSceneView;
            sceneView.pivot = controller.transform.position;
            sceneView.size = 10f;
            sceneView.Repaint();
        };
        condition = () => controller != null;
    }
}

public class DeleteInstanceAction : UnitDrawerItemAction
{
    public DeleteInstanceAction(UnitInstance unitInstance, UnitController controller, bool isInitial)
    {
        text = "Delete Instance";
        emoji = "❌";
        action = () => UnityEngine.Object.DestroyImmediate(unitInstance);
        condition = () => controller != null && !isInitial;
        backgroundColor = Color.red;
    }
}

public class TogglePartyAction : UnitDrawerItemAction
{
    public TogglePartyAction(bool isInParty, PartyInstanceService service, UnitInstance unitInstance)
    {
        text = isInParty ? "Remove from Party" : "Add to Party";
        emoji = "🎉";
        action = () =>
        {
            if (isInParty)
                service.RemoveUnitInstanceFromParty(unitInstance);
            else
                service.AddUnitInstanceToParty(unitInstance);
        };
        condition = () => true;
    }
}

public abstract class UnitDrawerDisplayItem
{
    public Func<bool> condition;
    public Color color;
    public Action drawAction;
}

public class InPartyDisplay : UnitDrawerDisplayItem
{
    public InPartyDisplay(bool isInParty, GUIStyle jobStyle)
    {
        condition = () => isInParty;
        color = Color.green;
        drawAction = () => EditorGUILayout.LabelField("🎉 In Party", jobStyle);
    }
}

public class BehaviourDisplay : UnitDrawerDisplayItem
{
    public BehaviourDisplay(string behaviour, GUIStyle jobStyle)
    {
        condition = () => !string.IsNullOrEmpty(behaviour);
        color = Color.yellow;
        drawAction = () => EditorGUILayout.LabelField("Behaviour: " + behaviour, jobStyle);
    }
}

public class InteractionDisplay : UnitDrawerDisplayItem
{
    public InteractionDisplay(string interaction, UnitController controller, GUIStyle jobStyle)
    {
        condition = () => !string.IsNullOrEmpty(interaction);
        color = Color.yellow;
        drawAction = () => EditorGUILayout.ObjectField("Interaction", controller?.CurrentInteraction, typeof(UnitInteraction), false);
    }
}

public class ActivityDisplay : UnitDrawerDisplayItem
{
    public ActivityDisplay(ActivityReference activity, GUIStyle jobStyle)
    {
        condition = () => activity != null;
        color = Color.magenta;
        drawAction = () => EditorGUILayout.ObjectField("Activity", activity, typeof(ActivityReference), false);
    }
}

public abstract class UnitListDrawer
{
    public static void DrawList(IEnumerable<UnitInstance> instances)
    {
        instances = instances
        .Where(instance => instance != null)
        .OrderBy(i => i.Id);

        // Load services once outside the loop for efficiency
        var unitInstanceService = Resources.FindObjectsOfTypeAll<UnitInstanceService>().FirstOrDefault();
        var partyInstanceService = Resources.FindObjectsOfTypeAll<PartyInstanceService>().FirstOrDefault();
        var unitControllerService = Resources.FindObjectsOfTypeAll<UnitControllerService>().FirstOrDefault();

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

                var initialUnit = unitInstanceService?.InitialUnits.Find(instance => instance.Id == unitInstance.Id);

                var controller = unitControllerService?.Controllers.Find(c => c.Instance != null && c.Instance.Id == unitInstance.Id);


                if (controller == null && SceneManager.GetActiveScene().name == "Gameplay")
                    throw new Exception("Controller is missing - reset game");

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
                var activityUnit = controller?.GetComponent<ActivityUnit>();
                var activity = activityUnit?.Activity;

                // Create action lists
                var menuItems = new List<UnitDrawerItemAction>
                {
                    new ViewAssetAction(unitInstance, initialUnit != null),
                    new ViewInstanceAction(unitInstance),
                    new TogglePartyAction(isInParty, partyInstanceService, unitInstance)
                };

                var shortcuts = new List<UnitDrawerItemAction>
                {
                    new ViewGameObjectAction(controller),
                    new DeleteInstanceAction(unitInstance, controller, initialUnit != null)
                };

                var displayItems = new List<UnitDrawerDisplayItem>
                {
                    new InPartyDisplay(isInParty, jobStyle),
                    new BehaviourDisplay(behaviour, jobStyle),
                    new InteractionDisplay(interaction, controller, jobStyle),
                    new ActivityDisplay(activity, jobStyle)
                };

                // Draw the unit
                DrawUnit(icon, displayName, job, backgroundColor, shortcuts, menuItems, jobStyle, controller, displayItems);
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox($"Error drawing unit {unitInstance?.Id ?? "unknown"}: {ex.Message}", MessageType.Error);
            }
        }
    }

    private static void DrawUnit(Texture icon, string displayName, string job, Color backgroundColor, List<UnitDrawerItemAction> shortcuts, List<UnitDrawerItemAction> menuItems, GUIStyle jobStyle, UnitController controller, List<UnitDrawerDisplayItem> displayItems)
    {
        EditorGUILayout.BeginHorizontal();
        Color originalBG = GUI.backgroundColor;
        GUI.backgroundColor = backgroundColor;
        Color originalColor = GUI.color;
        GUI.color = Color.white;
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        // Top: icon and info horizontal
        EditorGUILayout.BeginHorizontal();
        // Left side: icon
        EditorGUILayout.BeginVertical();
        // Draw icon
        if (icon != null)
        {
            GUIContent content = new GUIContent(icon);
            GUILayout.Label(content, GUILayout.Width(32), GUILayout.Height(32));
        }
        else
        {
            GUILayout.Label("No Icon", GUILayout.Width(32), GUILayout.Height(32));
        }
        EditorGUILayout.EndVertical();
        // Right side: name and job
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(displayName);
        EditorGUILayout.Space(-2);
        EditorGUILayout.LabelField(job, jobStyle);

        foreach (var displayItem in displayItems)
        {
            if (displayItem.condition())
            {
                GUI.color = displayItem.color;
                displayItem.drawAction();
                GUI.color = Color.white;
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();

        // Shortcuts
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

        // ... button
        if (GUILayout.Button("...", GUILayout.Width(20), GUILayout.Height(20)))
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
        EditorGUILayout.EndVertical();
        GUI.backgroundColor = originalBG;
        GUI.color = originalColor;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
    }
}

