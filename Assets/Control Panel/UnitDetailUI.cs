using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailUI : MonoBehaviour
{
    [Header("Services")]
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private UnitBehaviourPriorityService behaviourPriorityService;

    [Header("UI References")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button manualControlButton;
    [SerializeField] private Button unassignButton;
    [SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI speciesText;
    [SerializeField] private TextMeshProUGUI jobText;

    [Header("Runtime")]
    [SerializeField] private UnitInstance unitInstance;

    public UnitInstance UnitInstance => unitInstance;
    public event Action OnBackClicked;
    public event Action OnManualControlClicked;
    public event Action OnUnassignClicked;

    private void Awake()
    {
        if (backButton)
        {
            backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
        }

        if (manualControlButton)
        {
            manualControlButton.onClick.AddListener(() => OnManualControlClicked?.Invoke());
        }

        if (unassignButton)
        {
            unassignButton.onClick.AddListener(() => OnUnassignClicked?.Invoke());
        }
    }

    public void SetUnit(UnitInstance unit)
    {
        unitInstance = unit;

        if (unit != null)
        {
            // Check if unit is assigned to work
            bool isAssignedToWork = false;
            if (unitControllerService != null && behaviourPriorityService != null)
            {
                var controller = unitControllerService.Controllers.Find(c => c.Instance == unit);
                if (controller != null)
                {
                    isAssignedToWork = behaviourPriorityService.IsAssignedToWork(controller);
                }
            }

            // Show/hide buttons based on assignment status
            if (manualControlButton)
            {
                manualControlButton.gameObject.SetActive(!isAssignedToWork);
            }

            if (unassignButton)
            {
                unassignButton.gameObject.SetActive(isAssignedToWork);
            }

            if (nameText)
            {
                nameText.text = unit.DisplayName;
            }

            if (speciesText && unit.Species)
            {
                speciesText.text = unit.Species.name;
            }

            if (jobText && unit.Job)
            {
                jobText.text = unit.Job.ActionName;
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

            if (speciesText)
            {
                speciesText.text = "";
            }

            if (jobText)
            {
                jobText.text = "";
            }

            if (unitImage)
            {
                unitImage.sprite = null;
                unitImage.enabled = false;
            }
        }
    }
}
