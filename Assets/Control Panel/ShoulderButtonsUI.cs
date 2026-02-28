using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Shoulder buttons for cycling through units.
/// </summary>
public class ShoulderButtonsUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Optional Unit Icons")]
    [SerializeField] private bool showUnitIcons = false;
    [SerializeField] private Image leftUnitIcon;
    [SerializeField] private Image rightUnitIcon;

    public event UnityAction OnCyclePrevious;
    public event UnityAction OnCycleNext;

    private void Awake()
    {
        if (leftButton != null)
        {
            leftButton.onClick.AddListener(() => OnCyclePrevious?.Invoke());
        }

        if (rightButton != null)
        {
            rightButton.onClick.AddListener(() => OnCycleNext?.Invoke());
        }
    }

    public void UpdateIcons(UnitInstance leftUnit, UnitInstance rightUnit)
    {
        if (!showUnitIcons) return;

        if (leftUnitIcon != null)
        {
            if (leftUnit != null && leftUnit.Species != null && leftUnit.Species.Sprite != null)
            {
                leftUnitIcon.sprite = leftUnit.Species.Sprite;
                leftUnitIcon.enabled = true;
            }
            else
            {
                leftUnitIcon.enabled = false;
            }
        }

        if (rightUnitIcon != null)
        {
            if (rightUnit != null && rightUnit.Species != null && rightUnit.Species.Sprite != null)
            {
                rightUnitIcon.sprite = rightUnit.Species.Sprite;
                rightUnitIcon.enabled = true;
            }
            else
            {
                rightUnitIcon.enabled = false;
            }
        }
    }
}
