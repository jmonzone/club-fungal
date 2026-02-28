using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitRosterItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Selected State Visuals")]
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("Runtime")]
    [SerializeField] private UnitInstance unitInstance;
    [SerializeField] private bool isSelected;

    public UnitInstance UnitInstance => unitInstance;
    public bool IsSelected => isSelected;
    public event Action<UnitInstance> OnUnitClicked;

    private void Awake()
    {
        if (button)
        {
            button.onClick.AddListener(HandleButtonClick);
        }
    }

    private void HandleButtonClick()
    {
        if (unitInstance != null)
        {
            OnUnitClicked?.Invoke(unitInstance);
        }
    }

    public void SetUnit(UnitInstance unit)
    {
        unitInstance = unit;

        if (unit != null)
        {
            if (nameText)
            {
                nameText.text = unit.DisplayName;
            }

            if (unitImage && unit.Species)
            {
                unitImage.sprite = unit.Species.Sprite;
                unitImage.enabled = true;
            }
        }
        else
        {
            if (nameText)
            {
                nameText.text = "";
            }

            if (unitImage)
            {
                unitImage.sprite = null;
                unitImage.enabled = false;
            }
        }

        gameObject.SetActive(unit != null);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateSelectedVisuals();
    }

    private void UpdateSelectedVisuals()
    {
        // Update selected indicator
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(isSelected);
        }

        // Update background color
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }
}
