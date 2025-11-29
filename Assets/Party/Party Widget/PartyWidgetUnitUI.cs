using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyWidgetUnitUI : MonoBehaviour
{
    [SerializeField] private Image unitImage;
    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private UnitController unitController;

    public void SetUnit(UnitController unit)
    {
        unitController = unit;
        unitImage.sprite = unit.Instance.Data.Sprite;
        unitNameText.text = unit.Instance.DisplayName;
    }
}