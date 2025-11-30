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
        Debug.Log("Setting party widget unit UI to " + unit.DisplayName);
        unitInstance = unit;
        unitImage.sprite = unit.Data.Sprite;
        unitNameText.text = unit.DisplayName;
    }
}