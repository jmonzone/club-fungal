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
}
