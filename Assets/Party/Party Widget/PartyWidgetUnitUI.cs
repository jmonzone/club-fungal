using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyWidgetUnitUI : MonoBehaviour
{
    [SerializeField] private Image unitImage;
    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private UnitInstance unitInstance;

    public void SetUnit(UnitInstance unit)
    {
        unitInstance = unit;
        unitImage.sprite = unit.Species.Sprite;
        unitNameText.text = unit.DisplayName;
    }
}