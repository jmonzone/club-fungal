using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ActivityController : ActivityBehaviour
{
    public event UnityAction OnInitialized;
    public event UnityAction<ActivityUnit> OnPlayerEnterEvent;
    public event UnityAction<ActivityUnit> OnPlayerExitEvent;

    public void Initialize(ActivityReference activity)
    {
        Debug.Log($"[{name}] Initializing ActivityController with activity {activity.name}");
        SetActivity(activity);
        OnInitialized?.Invoke();
    }

    protected virtual void OnPlayerEnter(ActivityUnit player)
    {
        OnPlayerEnterEvent?.Invoke(player);
    }

    protected virtual void OnPlayerExit(ActivityUnit player)
    {
        OnPlayerExitEvent?.Invoke(player);
    }

}

public abstract class ActivityController<T> : ActivityController where T : ActivityUnitBehaviour
{
    [SerializeField] private int currentIndex;
    [SerializeField] private T currentUnit;
    [SerializeField] private List<T> units;

    protected bool PlayerIsActive => Activity.PlayerIsActive;
    protected bool PlayerIsSelected => currentUnit && currentUnit.IsPlayer;

    public T CurrentUnit => currentUnit;
    public T NextUnit => units[(currentIndex + 1) % units.Count];

    protected List<T> Units => units;

    public event UnityAction<T> OnUnitSelected;

    protected override void BindEvents()
    {
        Activity.OnActivityHasStarted += OnActivityStart;
        Activity.OnActivityHasEnded += OnActivityEnded;
        Activity.OnUnitEnter += OnUnitEnter;
        Activity.OnUnitExit += OnUnitExit;
        Activity.OnPlayerEnter += OnPlayerEnter;
        Activity.OnPlayerExit += OnPlayerExit;
    }

    protected override void UnbindEvents()
    {
        Activity.OnActivityHasStarted -= OnActivityStart;
        Activity.OnActivityHasEnded -= OnActivityEnded;
        Activity.OnUnitEnter -= OnUnitEnter;
        Activity.OnUnitExit -= OnUnitExit;
        Activity.OnPlayerEnter -= OnPlayerEnter;
        Activity.OnPlayerExit -= OnPlayerExit;
    }

    protected override void OnPlayerExit(ActivityUnit player)
    {
        base.OnPlayerExit(player);
        if (PlayerIsSelected) SelectNextUnit();
    }

    protected virtual void OnActivityStart()
    {
        Debug.Log($"[{name}] OnActivityStart");
        transform.position = Activity.Origin;

        currentIndex = -1;
        SelectNextUnit();
    }

    protected virtual void OnActivityEnded()
    {
        Debug.Log($"[{name}] OnActivityEnded");
    }

    protected void OnUnitEnter(ActivityUnit unit)
    {
        Debug.Log($"[{name}] OnUnitEnter {unit.name}");
        var activityBehaviour = unit.GetComponent<T>();
        // Priority system will automatically activate the activity behavior
        OnUnitBehaviourApplied(activityBehaviour);
    }

    protected void OnUnitExit(ActivityUnit unit)
    {
        Debug.Log($"[{name}] OnUnitExit {unit.name}");
        var activityBehaviour = unit.GetComponent<T>();
        OnUnitBehaviourRemoved(activityBehaviour);
    }

    protected virtual void OnUnitBehaviourApplied(T unit)
    {
        units.Add(unit);
    }


    protected virtual void OnUnitBehaviourRemoved(T unit)
    {
        units.Remove(unit);
    }

    public virtual void SelectUnit(T unit)
    {
        if (currentUnit == unit) return;

        UnselectUnit();

        currentIndex = Units.IndexOf(unit);
        currentUnit = unit;
        unit.OnSelect();
        OnUnitSelected?.Invoke(unit);
    }

    protected virtual void UnselectUnit()
    {
        if (currentUnit)
        {
            currentUnit.OnUnselect();
            currentUnit = null;
        }
    }

    [Obsolete("This script shouldn't handle unit selection directly. Use a separate UnitSelectionController instead.")]
    public void SelectNextUnit()
    {
        if (Activity.Units.Count == 0) return;
        currentIndex = (currentIndex + 1) % Activity.Units.Count;
        SelectUnit(units[currentIndex]);
    }
}
