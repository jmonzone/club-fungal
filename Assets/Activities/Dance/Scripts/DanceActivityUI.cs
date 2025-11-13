using System.Collections;
using UnityEngine;

public class DanceActivityUI : ActivityUI<DanceActivityUnit, DanceActivityController>
{
    [Header("References")]
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private DJTableReference djTableReference;
    [SerializeField] private DanceBackground background;
    [SerializeField] private DanceMoveUIManager danceMoveUIManager;
    [SerializeField] private PlayerActivityReference activityLevelUI;

    public override Camera Camera => background.DominantCamera;

    protected override void Awake()
    {
        base.Awake();
        danceMoveUIManager.Initialize();
        activityLevelUI.OnLevelUpHide += UpdateMovesUI;
    }

    protected override void OnPlayerEnter(ActivityUnit player)
    {
        base.OnPlayerEnter(player);
        UpdateMovesUI();

        background.StartDanceBackground();
    }

    protected override void OnPlayerExit(ActivityUnit player)
    {
        base.OnPlayerExit(player);
        background.EndDanceBackground();
        StopAllCoroutines();
    }

    protected override void OnUnitSelected(DanceActivityUnit unit)
    {
        base.OnUnitSelected(unit);
        UpdateMovesUI();
    }

    private void UpdateMovesUI()
    {
        if (Controller.CurrentUnit)
        {
            if (Controller.CurrentUnit.IsUsingDanceMove)
            {
                danceMoveUIManager.ToggleInteractable(false);
            }
            else
            {
                danceMoveUIManager.ToggleInteractable(Controller.CurrentUnit.IsPlayer);
            }

            if (Controller.CurrentUnit.IsPlayer)
            {
                var moves = Controller.CurrentUnit.Instance.Skills[Activity.PrimarySkill].Moves;
                StartCoroutine(danceMoveUIManager.Show(Controller.CurrentUnit, moves, () =>
                {
                    danceMoveUIManager.ToggleInteractable(false);
                    activityLevelUI.SetCanExit(false);
                },
                () =>
                {
                    Controller.SelectNextUnit();
                    activityLevelUI.SetCanExit(true);
                }));
            }
            else
            {
                danceMoveUIManager.Hide();
            }
        }
    }
}
