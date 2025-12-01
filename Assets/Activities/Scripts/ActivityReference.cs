using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ActivityReference", menuName = "Club Fungal/Activities/Activity Reference")]
public class ActivityReference : ScriptableObject
{
    [Header("References")]
    [SerializeField] private ViewReference activityView;
    [SerializeField] private Skill primarySkill;
    [SerializeField] private ActivityController activityControllerPrefab;

    [Header("Runtime")]
    [SerializeField] private Vector3 origin;
    [SerializeField] private ActivityUnit player;
    [SerializeField] private List<ActivityUnit> units = new List<ActivityUnit>();

    public ViewReference ViewReference => activityView;
    public Skill PrimarySkill => primarySkill;

    public Vector3 Origin => origin;
    public bool PlayerIsActive => player;
    public List<ActivityUnit> Units => units;

    public event UnityAction OnActivityHasStarted;
    public event UnityAction OnActivityHasEnded;

    public event UnityAction<ActivityUnit> OnUnitEnter;
    public event UnityAction<ActivityUnit> OnUnitExit;

    public event UnityAction<OnXpIncreasedEventArgs> OnXPIncreased;

    public event UnityAction<ActivityUnit> OnPlayerEnter;
    public event UnityAction<ActivityUnit> OnPlayerExit;

    public void StartActivity(Vector3 origin, List<UnitController> units)
    {
        var activityController = Instantiate(activityControllerPrefab, origin, Quaternion.identity);
        activityController.Initialize(this);

        Debug.Log($"Starting activity {name}");
        this.origin = origin;
        this.units = new List<ActivityUnit>();

        foreach (var unit in units)
        {
            AddUnit(unit.GetComponent<ActivityUnit>());
        }

        OnActivityHasStarted?.Invoke();
    }

    public void EndActivity()
    {
        if (PlayerIsActive) ExitActivity(player);

        var unitsToRemove = new List<ActivityUnit>(Units);
        foreach (var unit in unitsToRemove)
        {
            RemoveUnit(unit);
        }

        OnActivityHasEnded?.Invoke();
        units = new List<ActivityUnit>();
    }

    public void EnterActivity(ActivityUnit player)
    {
        this.player = player;
        AddUnit(player);
        OnPlayerEnter?.Invoke(player);
    }

    public void ExitActivity(ActivityUnit player)
    {
        this.player = null;
        RemoveUnit(player);
        OnPlayerExit?.Invoke(player);
    }

    public void AddUnit(ActivityUnit unit)
    {
        unit.JoinActivity(this);
        units.Add(unit);
        unit.OnXPIncreased += Unit_OnXPIncreased;
        UpdateUnits();
        OnUnitEnter?.Invoke(unit);
    }

    public void RemoveUnit(ActivityUnit unit)
    {
        unit.ExitActivity();
        units.Remove(unit);
        unit.OnXPIncreased -= Unit_OnXPIncreased;
        UpdateUnits();
        OnUnitExit?.Invoke(unit);
    }

    private void Unit_OnXPIncreased(OnXpIncreasedEventArgs args)
    {
        OnXPIncreased?.Invoke(args);
    }

    private void UpdateUnits()
    {
        UnitController.ArrangeUnitsInRadius(origin, units.Select(unit => unit.Controller).ToList());

        foreach (var unit in units)
        {
            unit.SetActivityPosition(unit.Controller.Destination.Destination);
        }
    }
}