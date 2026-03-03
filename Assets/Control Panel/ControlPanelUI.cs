using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages visibility of UI panels using a generic view discovery system.
/// Views self-register their control modes and handle their own event subscriptions.
/// </summary>
public class ControlPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ControlModeService controlModeService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private GameObject renderRoot;

    private ControlPanelViewUI[] allViews;
    private Dictionary<ControlModeService.ControlMode, ControlPanelViewUI> modeToViewMap;

    private void Awake()
    {
        // Get all views as children (including inactive)
        allViews = GetComponentsInChildren<ControlPanelViewUI>(includeInactive: true);

        // Build mapping from control modes to views
        modeToViewMap = new Dictionary<ControlModeService.ControlMode, ControlPanelViewUI>();
        foreach (var view in allViews)
        {
            // Initialize each view with service references
            view.Initialize(controlModeService, unitControllerService);

            // Map each supported mode to this view
            foreach (var mode in view.SupportedModes)
            {
                modeToViewMap[mode] = view;
            }
        }

        // Subscribe to service events
        controlModeService.OnModeChanged += HandleModeChanged;
        controlModeService.OnShowAssignUnitUI += HandleShowAssignUnitUI;
        controlModeService.OnHideAssignUnitUI += HandleHideAssignUnitUI;
    }

    private void Start()
    {
        HandleModeChanged(controlModeService.CurrentMode);
    }

    private void OnDestroy()
    {
        // Cleanup all views
        foreach (var view in allViews)
        {
            view.Cleanup();
        }

        // Unsubscribe from service events
        controlModeService.OnModeChanged -= HandleModeChanged;
        controlModeService.OnShowAssignUnitUI -= HandleShowAssignUnitUI;
        controlModeService.OnHideAssignUnitUI -= HandleHideAssignUnitUI;
    }

    private void HandleModeChanged(ControlModeService.ControlMode mode)
    {
        // Find view for this mode
        if (modeToViewMap.TryGetValue(mode, out var viewToShow))
        {
            bool showRenderRoot = mode != ControlModeService.ControlMode.FreeCamera;
            ShowViewAndHideOthers(viewToShow, showRenderRoot);
        }
    }

    private void HandleShowAssignUnitUI(UnitController building, AssignUnitAction action)
    {
        // Find AssignUnitSelectionUI generically from allViews
        var assignUI = System.Array.Find(allViews, v => v is AssignUnitSelectionUI) as AssignUnitSelectionUI;
        if (assignUI != null)
        {
            ShowViewAndHideOthers(assignUI, showRenderRoot: true);
            assignUI.ShowForAssignment(building, action);
        }
    }

    private void HandleHideAssignUnitUI()
    {
        // Return to previous view based on current mode
        HandleModeChanged(controlModeService.CurrentMode);
    }

    /// <summary>
    /// Shows the specified view and hides all others.
    /// </summary>
    private void ShowViewAndHideOthers(ControlPanelViewUI viewToShow, bool showRenderRoot)
    {
        if (renderRoot != null)
        {
            renderRoot.SetActive(showRenderRoot);
        }

        foreach (var view in allViews)
        {
            if (view == viewToShow)
            {
                view.Show();
            }
            else
            {
                view.Hide();
            }
        }
    }
}
