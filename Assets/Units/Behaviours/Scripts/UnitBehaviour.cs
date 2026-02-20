using UnityEngine;
using UnityEngine.Events;

public abstract class UnitComponent : MonoBehaviour
{
    public UnitController Controller { get; private set; }
    public UnitInstance Instance => Controller.Instance;

    protected virtual void Awake()
    {
        Controller = GetComponentInParent<UnitController>();
        Controller.OnInitialized += OnInitialized;
    }

    protected virtual void OnInitialized()
    {
    }
}

public abstract class UnitBehaviour : UnitComponent
{
    [SerializeField] private bool isActive;
    public bool IsActive => isActive;

    protected virtual void Update()
    {

    }

    public virtual void StartBehaviour()
    {
        if (!isActive)
        {
            isActive = true;
            OnBehaviourStart();
        }
    }

    protected virtual void OnBehaviourStart()
    {
    }

    public virtual void StopBehaviour()
    {
        if (isActive)
        {
            isActive = false;
            OnBehaviourStop();
        }
    }

    protected virtual void OnBehaviourStop()
    {
    }

    public virtual void PauseBehaviour()
    {
    }

    public virtual void UnpauseBehaviour()
    {
    }
}