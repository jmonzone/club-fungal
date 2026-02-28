using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI component for manual control mode.
/// Managed by ControlPanelUI.
/// </summary>
public class UnitControllerUI : MonoBehaviour
{
    public enum UnitSwitcherMode
    {
        ShoulderButtons,
        ScrollView
    }

    [Header("Settings")]
    [SerializeField] private UnitSwitcherMode switcherMode = UnitSwitcherMode.ShoulderButtons;

    [Header("Buttons")]
    [SerializeField] private Button backButton;

    [Header("Unit Switcher Components")]
    [SerializeField] private ShoulderButtonsUI shoulderButtons;
    [SerializeField] private UnitSwitcherScrollUI scrollView;

    public event UnityAction OnBackClicked;
    public event UnityAction OnCyclePrevious;
    public event UnityAction OnCycleNext;
    public event UnityAction<UnitInstance> OnUnitSelected;

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        if (shoulderButtons != null)
        {
            shoulderButtons.OnCyclePrevious += () => OnCyclePrevious?.Invoke();
            shoulderButtons.OnCycleNext += () => OnCycleNext?.Invoke();
        }

        if (scrollView != null)
        {
            scrollView.OnUnitSelected += (unit) => OnUnitSelected?.Invoke(unit);
        }

        UpdateSwitcherMode();
    }

    private void UpdateSwitcherMode()
    {
        if (shoulderButtons != null)
        {
            shoulderButtons.gameObject.SetActive(switcherMode == UnitSwitcherMode.ShoulderButtons);
        }

        if (scrollView != null)
        {
            scrollView.gameObject.SetActive(switcherMode == UnitSwitcherMode.ScrollView);
        }
    }

    public void UpdateShoulderButtons(UnitInstance previousUnit, UnitInstance nextUnit)
    {
        if (shoulderButtons != null && switcherMode == UnitSwitcherMode.ShoulderButtons)
        {
            shoulderButtons.UpdateIcons(previousUnit, nextUnit);
        }
    }

    public void UpdateScrollView(UnitInstance selectedUnit)
    {
        if (scrollView != null && switcherMode == UnitSwitcherMode.ScrollView)
        {
            scrollView.RefreshList(selectedUnit);
        }
    }
}
