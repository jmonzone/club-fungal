using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class PlayerActivityReference : ScriptableObject
{
    [Header("References")]
    [SerializeField] private PlayerService playerReference;
    [SerializeField] private Navigation navigation;
    [SerializeField] private ViewReference levelUpView;

    [Header("Runtime")]
    [SerializeField] private ActivityReference activity;
    public List<ActivityUnit> Units => activity.Units;

    public event UnityAction<ActivityUnit> OnUnitEnter;
    public event UnityAction<ActivityUnit> OnPlayerEnter;
    public event UnityAction<ActivityUnit> OnPlayerExit;
    public event UnityAction<OnXpIncreasedEventArgs> OnXPIncreased;

    public event UnityAction<ActivityUnit> OnLevelUp;

    public void EnterActivity(ActivityReference activity)
    {
        this.activity = activity;

        activity.OnUnitEnter += OnUnitEnter;
        activity.OnPlayerEnter += OnPlayerEnter;
        activity.OnPlayerExit += OnPlayerExit;
        activity.OnXPIncreased += OnXPIncreased;

        navigation.Navigate(activity.ViewReference);

        activity.EnterActivity(playerReference.Player.GetComponent<ActivityUnit>());
    }

    public void ExitActivity()
    {
        navigation.GoBackToRoot();
        activity.ExitActivity(playerReference.ActivityUnit);

        activity.OnUnitEnter -= OnUnitEnter;
        activity.OnPlayerEnter -= OnPlayerEnter;
        activity.OnPlayerExit -= OnPlayerExit;
        activity.OnXPIncreased -= OnXPIncreased;

        activity = null;
    }

    public void OnUnitLevelUp(ActivityUnit unit)
    {
        navigation.Navigate(levelUpView);
        OnLevelUp?.Invoke(unit);
    }
}
