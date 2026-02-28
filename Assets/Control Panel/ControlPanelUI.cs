using UnityEngine;

public class ControlPanelUI : MonoBehaviour
{
    [Header("Views")]
    [SerializeField] private UnitRosterUI unitRosterUI;
    [SerializeField] private UnitDetailUI unitDetailUI;

    private UnitInstance selectedUnit;

    private void Awake()
    {
        if (unitRosterUI)
        {
            unitRosterUI.OnUnitSelected += ShowUnitDetail;
        }

        if (unitDetailUI)
        {
            unitDetailUI.OnBackClicked += ShowRoster;
        }
    }

    private void Start()
    {
        ShowRoster();
    }

    private void OnDestroy()
    {
        if (unitRosterUI)
        {
            unitRosterUI.OnUnitSelected -= ShowUnitDetail;
        }

        if (unitDetailUI)
        {
            unitDetailUI.OnBackClicked -= ShowRoster;
        }
    }

    public void ShowRoster()
    {
        if (unitRosterUI)
        {
            unitRosterUI.gameObject.SetActive(true);
        }

        if (unitDetailUI)
        {
            unitDetailUI.gameObject.SetActive(false);
        }
    }

    public void ShowUnitDetail(UnitInstance unit)
    {
        selectedUnit = unit;

        if (unitRosterUI)
        {
            unitRosterUI.gameObject.SetActive(false);
        }

        if (unitDetailUI)
        {
            unitDetailUI.SetUnit(unit);
            unitDetailUI.gameObject.SetActive(true);
        }
    }
}
