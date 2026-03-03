using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI component for proximity actions showing progress, item icon, and recruit button.
/// </summary>
public class ProximityActionUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private ButtonUI buttonUI;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI progressText;

    public ButtonUI ButtonUI => buttonUI;
    public Slider ProgressSlider => progressSlider;
    public Image ItemIcon => itemIcon;
    public TextMeshProUGUI ProgressText => progressText;

    private void Awake()
    {
        if (buttonUI == null) buttonUI = GetComponent<ButtonUI>();
        if (progressSlider == null) progressSlider = GetComponentInChildren<Slider>();
        if (itemIcon == null) itemIcon = transform.Find("Icon")?.GetComponent<Image>();
        if (progressText == null) progressText = GetComponentInChildren<TextMeshProUGUI>();
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

    public void UpdateProgress(int current, int required)
    {
        bool canAfford = current >= required;

        // Update slider
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = required;
            progressSlider.value = current;
        }

        // Update text
        if (progressText != null)
        {
            progressText.text = $"{current}/{required}";
        }

        // Enable/disable button
        if (buttonUI != null && buttonUI.GetComponent<Button>() != null)
        {
            buttonUI.GetComponent<Button>().interactable = canAfford;
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

        // Hide progress elements if they exist
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);
        }

        if (progressText != null)
        {
            progressText.gameObject.SetActive(false);
        }
    }
}
