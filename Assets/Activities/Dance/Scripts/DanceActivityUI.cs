using UnityEngine;
using UnityEngine.UI;

public class DanceActivityUI : ActivityUI<DanceActivityUnit, DanceActivityController>
{
    [Header("Dance References")]
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private DJTableReference djTableReference;
    [SerializeField] private DanceBackground background;
    [SerializeField] private DanceMoveUIManager danceMoveUIManager;

    public override Camera Camera => background.DominantCamera;

    protected override void Awake()
    {
        base.Awake();
        danceMoveUIManager.Initialize();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateMovesUI();
    }

    protected override void OnUnitEnter(ActivityUnit unit)
    {
        base.OnUnitEnter(unit);
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
                    BackButton.interactable = false;
                },
                () =>
                {
                    Controller.SelectNextUnit();
                    BackButton.interactable = true;
                }));
            }
            else
            {
                danceMoveUIManager.Hide();
            }
        }
    }
}
