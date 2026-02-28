using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI component for manual control mode.
/// Managed by ControlPanelUI.
/// </summary>
public class UnitControllerUI : MonoBehaviour
{
    [SerializeField] private Button backButton;

    public event UnityAction OnBackClicked;

    private void Awake()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }
    }
}
