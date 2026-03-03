using UnityEngine;

/// <summary>
/// Base class for all control panel view UIs.
/// Provides common Show/Hide functionality and control mode mapping.
/// </summary>
public abstract class ControlPanelViewUI : MonoBehaviour
{
    /// <summary>
    /// Which control mode(s) trigger this view to be shown.
    /// Override this property in derived classes.
    /// </summary>
    public abstract ControlModeService.ControlMode[] SupportedModes { get; }

    /// <summary>
    /// Initialize the view with service references.
    /// Called once during ControlPanelUI.Awake().
    /// Override to subscribe to events, store service references, etc.
    /// </summary>
    public virtual void Initialize(ControlModeService controlModeService, UnitControllerService unitControllerService)
    {
    }

    /// <summary>
    /// Called when this view becomes active.
    /// Override to update UI with current data.
    /// </summary>
    public virtual void OnShown()
    {
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        OnShown();
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called when the view is being destroyed.
    /// Override to unsubscribe from events, clean up, etc.
    /// </summary>
    public virtual void Cleanup()
    {
    }
}
