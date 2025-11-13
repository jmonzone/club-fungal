using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class PlayerActivityReference : ScriptableObject
{
    [SerializeField] private ActivityReference activity;
    [SerializeField] private PlayerReference playerReference;
    [SerializeField] private bool canExit;
    [SerializeField] private Navigation navigation;
    [SerializeField] private ViewReference activityLevelView;

    public List<ActivityUnit> Units => activity.Units;
    public bool CanExit => canExit;

    public event UnityAction<ActivityUnit> OnUnitEnter;
    public event UnityAction<ActivityUnit> OnPlayerEnter;
    public event UnityAction<ActivityUnit> OnPlayerExit;
    public event UnityAction<OnXpIncreasedEventArgs> OnXPIncreased;

    public event UnityAction OnLevelUpHide;
    public event UnityAction<bool> OnCanExitChanged;

    public void EnterActivity(ActivityReference activity)
    {
        this.activity = activity;

        activity.OnUnitEnter += OnUnitEnter;
        activity.OnPlayerEnter += OnPlayerEnter;
        activity.OnPlayerExit += OnPlayerExit;
        activity.OnXPIncreased += OnXPIncreased;

        activity.EnterActivity(playerReference.Player.GetComponent<ActivityUnit>());
        navigation.Navigate(activity.ViewReference);
        activityLevelView.Show();
    }

    public void ExitActivity()
    {
        activityLevelView.RequestHide();
        navigation.GoBackToRoot();
        activity.ExitActivity(playerReference.ActivityUnit);

        activity.OnUnitEnter -= OnUnitEnter;
        activity.OnPlayerEnter -= OnPlayerEnter;
        activity.OnPlayerExit -= OnPlayerExit;
        activity.OnXPIncreased -= OnXPIncreased;

        activity = null;
    }

    public void HideLevelUp()
    {
        OnLevelUpHide?.Invoke();
    }

    public void SetCanExit(bool value)
    {
        canExit = value;
        OnCanExitChanged?.Invoke(value);
    }
}
