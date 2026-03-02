using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI item for a single unit in the assignment selection list.
/// Shows portrait, name, job status, and availability indicator.
/// </summary>
public class AssignUnitItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image unitPortrait;
    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI jobStatusText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject availabilityIndicator;

    [Header("Availability Colors")]
    [SerializeField] private Color availableColor = new Color(0.4f, 0.8f, 0.4f); // Green
    [SerializeField] private Color assignedColor = new Color(0.8f, 0.8f, 0.4f); // Yellow
    [SerializeField] private Color unavailableColor = new Color(0.6f, 0.6f, 0.6f); // Gray

    [Header("Runtime")]
    private UnitController unitController;
    private UnitAvailability availability;

    public event Action<UnitController> OnUnitClicked;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(HandleButtonClick);
        }
    }

    /// <summary>
    /// Initialize the item with unit data and availability status.
    /// </summary>
    public void Initialize(UnitController unit, UnitAvailability availability)
    {
        this.unitController = unit;
        this.availability = availability;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (unitController == null || unitController.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // Set portrait
        if (unitPortrait != null)
        {
            unitPortrait.sprite = unitController.Instance.Species?.Sprite;
            unitPortrait.enabled = unitController.Instance.Species?.Sprite != null;
        }

        // Set name
        if (unitNameText != null)
        {
            unitNameText.text = unitController.Instance.DisplayName;
        }

        // Set job status text
        if (jobStatusText != null)
        {
            if (unitController.Instance.Job != null)
            {
                jobStatusText.text = $"({unitController.Instance.Job.ActionName})";
            }
            else
            {
                jobStatusText.text = "(Available)";
            }
        }

        // Update visual indicators based on availability
        UpdateAvailabilityVisuals();
    }

    private void UpdateAvailabilityVisuals()
    {
        Color indicatorColor = availableColor;
        bool isInteractable = true;

        switch (availability)
        {
            case UnitAvailability.Available:
                indicatorColor = availableColor;
                isInteractable = true;
                break;

            case UnitAvailability.AssignedElsewhere:
                indicatorColor = assignedColor;
                isInteractable = true; // Can still reassign
                break;

            case UnitAvailability.Unavailable:
                indicatorColor = unavailableColor;
                isInteractable = false;
                break;
        }

        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = indicatorColor;
        }

        // Update availability indicator
        if (availabilityIndicator != null)
        {
            availabilityIndicator.SetActive(availability == UnitAvailability.Available);
        }

        // Update button interactability
        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }

    private void HandleButtonClick()
    {
        if (unitController != null)
        {
            OnUnitClicked?.Invoke(unitController);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleButtonClick);
        }
    }
}
