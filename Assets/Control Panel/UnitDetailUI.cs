using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitDetailUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button manualControlButton;
    [SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI speciesText;
    [SerializeField] private TextMeshProUGUI jobText;

    [Header("Runtime")]
    [SerializeField] private UnitInstance unitInstance;

    public UnitInstance UnitInstance => unitInstance;
    public event Action OnBackClicked;
    public event Action OnManualControlClicked;

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
