using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI component for proximity actions showing item icon and button.
/// </summary>
public class ProximityActionUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private ButtonUI buttonUI;
    [SerializeField] private Image itemIcon;

    public ButtonUI ButtonUI => buttonUI;
    public Image ItemIcon => itemIcon;

    private void Awake()
    {
        if (buttonUI == null) buttonUI = GetComponent<ButtonUI>();
        if (itemIcon == null) itemIcon = transform.Find("Icon")?.GetComponent<Image>();
    }

    public void Initialize(string actionName, Sprite icon, UnityAction onButtonClick)
    {
        // Initialize button with action name
        if (buttonUI != null)
        {
            buttonUI.Initialize(actionName, onButtonClick);
        }

        // Set item icon
        if (itemIcon != null && icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.gameObject.SetActive(true);
        }
        else if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the UI to show an assigned unit instead of the action button.
    /// Used when a building has a worker assigned to it.
    /// </summary>
    public void UpdateToAssignedState(Sprite unitPortrait, string unitName)
    {
        // Update icon to show unit portrait
        if (itemIcon != null && unitPortrait != null)
        {
            itemIcon.sprite = unitPortrait;
            itemIcon.gameObject.SetActive(true);
        }

        // Update button text to show unit name
        if (buttonUI != null)
        {
            buttonUI.Initialize(unitName, null);
            buttonUI.gameObject.SetActive(true);
        }
    }
}
