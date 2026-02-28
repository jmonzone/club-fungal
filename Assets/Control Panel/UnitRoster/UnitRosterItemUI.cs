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

    [Header("Runtime")]
    [SerializeField] private UnitInstance unitInstance;

    public UnitInstance UnitInstance => unitInstance;
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
}
