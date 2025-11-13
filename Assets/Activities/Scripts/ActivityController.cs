using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ActivityController<T> : MonoBehaviour where T : ActivityBehaviour
{
    [Header("Activity References")]
    [SerializeField] private ActivityReference activity;
    [SerializeField] private Skill primarySkill;

    [Header("Activity Runtime")]
    [SerializeField] private T player;
    [SerializeField] private int currentIndex;
    [SerializeField] private T currentUnit;
    [SerializeField] private List<T> units;

    protected ActivityReference Activity => activity;
    protected Skill PrimarySkill => primarySkill;
    protected bool PlayerIsActive => activity.PlayerIsActive;
    protected bool PlayerIsSelected => currentUnit && currentUnit.IsPlayer;

    protected T Player => player;

    public T CurrentUnit => currentUnit;
    public T NextUnit => units[(currentIndex + 1) % units.Count];

    protected List<T> Units => units;

    public event UnityAction<T> OnUnitSelected;

    protected virtual void Awake()
    {
    }

    private void OnEnable()
    {
        activity.OnActivityHasStarted += OnActivityStart;
        activity.OnActivityHasEnded += OnActivityEnded;
        activity.OnUnitEnter += OnUnitEnter;
        activity.OnUnitExit += OnUnitExit;
        activity.OnPlayerEnter += OnPlayerEnter;
        activity.OnPlayerExit += OnPlayerExit;
    }

    private void OnDisable()
    {
        activity.OnActivityHasStarted -= OnActivityStart;
        activity.OnActivityHasEnded -= OnActivityEnded;
        activity.OnUnitEnter -= OnUnitEnter;
        activity.OnUnitExit -= OnUnitExit;
        activity.OnPlayerEnter -= OnPlayerEnter;
        activity.OnPlayerExit -= OnPlayerExit;
    }

    protected virtual void OnPlayerEnter(ActivityUnit player)
    {
        this.player = player.GetComponent<T>();

        //currentIndex = units.FindIndex(unit => unit == player);
        //SelectUnit(player.GetComponent<T>());
    }

    protected virtual void OnPlayerExit(ActivityUnit player)
    {
        if (PlayerIsSelected) SelectNextUnit();
    }

    protected virtual void OnActivityStart()
    {
        Debug.Log($"[{name}] OnActivityStart");
        transform.position = activity.Origin;

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
        unit.SetBehaviour(activityBehaviour);
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

    //todo: separate unit selection to another component
    //it is not ActivityController specific
    public void SelectNextUnit()
    {
        if (Activity.Units.Count == 0) return;
        currentIndex = (currentIndex + 1) % Activity.Units.Count;
        SelectUnit(units[currentIndex]);
    }
}
