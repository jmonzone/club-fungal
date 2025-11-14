using System.Collections.Generic;
using UnityEngine;

public class ActivityLevelUI : MonoBehaviour
{
    [SerializeField] private PlayerActivityReference playerActivity;

    private List<SkillLevelUI> skillLevelUIViews;

    public Dictionary<ActivityUnit, SkillLevelUI> UnitLevelViewMap { get; private set; } = new Dictionary<ActivityUnit, SkillLevelUI>();

    private void OnEnable()
    {
        Show(playerActivity.Units);
        playerActivity.OnUnitEnter += OnUnitEnter;
        playerActivity.OnPlayerEnter += OnPlayerEnter;
        playerActivity.OnPlayerExit += OnPlayerExit;
    }

    private void OnDisable()
    {
        playerActivity.OnUnitEnter -= OnUnitEnter;
        playerActivity.OnPlayerEnter -= OnPlayerEnter;
        playerActivity.OnPlayerExit -= OnPlayerExit;
    }

    private void OnUnitEnter(ActivityUnit unit)
    {
        Show(playerActivity.Units);
    }

    private void OnPlayerEnter(ActivityUnit player)
    {
        playerActivity.OnXPIncreased += OnXPIncreased;
    }

    private void OnPlayerExit(ActivityUnit player)
    {
        playerActivity.OnXPIncreased -= OnXPIncreased;
    }

    private void OnXPIncreased(OnXpIncreasedEventArgs args)
    {
        //Debug.Log("ActivityUI.OnXPIncreased");
        var unitWorldPos = args.SourcePosition;
        var unitScreenPos = Camera.main.WorldToScreenPoint(unitWorldPos);
        UnitLevelViewMap[args.Unit].SetColor(args.Unit.Color);
        UnitLevelViewMap[args.Unit].Increase(args.XP, unitScreenPos, hasLeveledUp =>
        {
            if (hasLeveledUp)
            {
                playerActivity.OnUnitLevelUp(args.Unit);
            }
        });
    }

    public void Show(IEnumerable<ActivityUnit> units)
    {
        gameObject.SetActive(true);

        skillLevelUIViews = new List<SkillLevelUI>();
        GetComponentsInChildren(true, skillLevelUIViews);

        UnitLevelViewMap = new Dictionary<ActivityUnit, SkillLevelUI>();

        var i = 0;
        foreach (var unit in units)
        {
            skillLevelUIViews[i].SetUnit(unit);
            skillLevelUIViews[i].gameObject.SetActive(true);

            UnitLevelViewMap.Add(unit, skillLevelUIViews[i]);
            i++;
        }

        while (i < skillLevelUIViews.Count)
        {
            skillLevelUIViews[i].gameObject.SetActive(false);
            i++;
        }
    }
}
