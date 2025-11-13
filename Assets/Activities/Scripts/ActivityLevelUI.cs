using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ActivityLevelUI : MonoBehaviour
{
    [SerializeField] private PlayerActivityReference activityUIReference;
    [SerializeField] private PlayerReference playerReference;

    [SerializeField] private SkillLevelUIManager levelUI;
    [SerializeField] private FadeCanvasGroup gameplayUI;
    [SerializeField] private LevelUpUI levelUpUI;
    [SerializeField] private Button exitButton;

    private void Awake()
    {
        exitButton.onClick.AddListener(activityUIReference.ExitActivity);
        levelUpUI.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        activityUIReference.OnUnitEnter += OnUnitEnter;
        activityUIReference.OnPlayerEnter += OnPlayerEnter;
        activityUIReference.OnPlayerExit += OnPlayerExit;
        activityUIReference.OnCanExitChanged += SetExitButtonInteractable;
    }

    private void OnDisable()
    {
        activityUIReference.OnUnitEnter -= OnUnitEnter;
        activityUIReference.OnPlayerEnter -= OnPlayerEnter;
        activityUIReference.OnPlayerExit -= OnPlayerExit;
        activityUIReference.OnCanExitChanged -= SetExitButtonInteractable;
    }

    private void OnUnitEnter(ActivityUnit unit)
    {
        levelUI.Show(activityUIReference.Units);
    }

    private void OnPlayerEnter(ActivityUnit player)
    {
        activityUIReference.OnXPIncreased += OnXPIncreased;

        StartCoroutine(gameplayUI.FadeIn());
    }

    private void OnPlayerExit(ActivityUnit player)
    {
        activityUIReference.OnXPIncreased -= OnXPIncreased;
    }

    private void OnXPIncreased(OnXpIncreasedEventArgs args)
    {
        if (levelUI.gameObject.activeInHierarchy)
        {
            //Debug.Log("ActivityUI.OnXPIncreased");
            var unitWorldPos = args.SourcePosition;
            var unitScreenPos = Camera.main.WorldToScreenPoint(unitWorldPos);
            levelUI.UnitLevelViewMap[args.Unit].SetColor(args.Unit.Color);
            levelUI.UnitLevelViewMap[args.Unit].Increase(args.XP, unitScreenPos, hasLeveledUp =>
            {
                if (hasLeveledUp)
                {
                    StartCoroutine(LevelUpRoutine(args.Unit));
                }
            });
        }
    }

    private IEnumerator LevelUpRoutine(ActivityUnit unit)
    {
        yield return gameplayUI.FadeOut();
        yield return levelUpUI.Show(unit, () =>
        {
            StartCoroutine(LevelUI_OnExitRoutine());
        });
    }

    private IEnumerator LevelUI_OnExitRoutine()
    {
        activityUIReference.HideLevelUp();

        yield return levelUpUI.Hide();
        yield return gameplayUI.FadeIn();
        levelUI.Show(activityUIReference.Units);
    }

    private void SetExitButtonInteractable(bool value)
    {
        exitButton.interactable = value;
    }
}
