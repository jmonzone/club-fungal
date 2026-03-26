using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI component for proximity actions showing button.
/// </summary>
public class ProximityActionUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private ButtonUI buttonUI;

    public ButtonUI ButtonUI => buttonUI;

    private void Awake()
    {
        if (buttonUI == null) buttonUI = GetComponent<ButtonUI>();
    }

    public void Initialize(string actionName, UnityAction onButtonClick)
    {
        // Initialize button with action name
        if (buttonUI != null)
        {
            buttonUI.Initialize(actionName, onButtonClick);
        }
    }

    /// <summary>
    /// Updates the UI to show an assigned unit instead of the action button.
    /// Used when a building has a worker assigned to it.
    /// </summary>
    public void UpdateToAssignedState(string unitName)
    {
        // Update button text to show unit name
        if (buttonUI != null)
        {
            buttonUI.Initialize(unitName, null);
            buttonUI.gameObject.SetActive(true);
        }
    }
}
