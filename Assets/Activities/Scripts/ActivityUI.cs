using UnityEngine;
using UnityEngine.UI;

public abstract class ActivityBehaviour : MonoBehaviour
{
    [SerializeField] private ActivityReference activity;

    public ActivityReference Activity => activity;

    protected virtual void Awake()
    {
    }

    protected virtual void OnEnable()
    {
        BindEvents();
    }

    protected virtual void OnDisable()
    {
        UnbindEvents();
    }

    protected virtual void BindEvents()
    {
        if (activity == null) return;
    }

    protected virtual void UnbindEvents()
    {
        if (activity == null) return;
    }

    protected void SetActivity(ActivityReference newActivity)
    {
        if (activity != null)
        {
            UnbindEvents();
        }

        activity = newActivity;

        if (activity != null)
        {
            BindEvents();
        }
    }
}

public abstract class ActivityComponent : ActivityBehaviour
{
    protected override void Awake()
    {
        base.Awake();

        var activityController = GetComponentInParent<ActivityController>();

        if (activityController.Activity)
        {
            SetActivity(activityController.Activity);
        }

        activityController.OnInitialized += () =>
        {
            SetActivity(activityController.Activity);
        };
    }
}

public abstract class ActivityUI : ActivityComponent
{
    private Camera mainCamera;

    public virtual Camera Camera => mainCamera;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }
}

public abstract class ActivityUI<T1, T2> : ActivityUI where T1 : ActivityUnitBehaviour where T2 : ActivityController<T1>
{
    [Header("Activity References")]
    [SerializeField] private PlayerActivityReference playerActivityReference;
    [SerializeField] private T2 controller;
    [SerializeField] private Button backButton;

    [Header("Activity Runtime")]
    [SerializeField] private T1 player;

    protected T1 Player => player;
    protected T2 Controller => controller;
    protected Button BackButton => backButton;
    protected bool PlayerIsSelected => player == controller.CurrentUnit;

    protected override void Awake()
    {
        base.Awake();
        BackButton.onClick.AddListener(playerActivityReference.ExitActivity);
    }

    protected override void BindEvents()
    {

        base.BindEvents();
        Activity.OnUnitEnter += OnUnitEnter;
        Activity.OnUnitExit += OnUnitExit;
        Activity.OnPlayerEnter += OnPlayerEnter;
        Activity.OnPlayerExit += OnPlayerExit;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Activity.OnUnitEnter -= OnUnitEnter;
        Activity.OnUnitExit -= OnUnitExit;
        Activity.OnPlayerEnter -= OnPlayerEnter;
        Activity.OnPlayerExit -= OnPlayerExit;
    }

    protected virtual void OnUnitEnter(ActivityUnit unit)
    {

    }

    protected virtual void OnUnitExit(ActivityUnit unit)
    {
    }

    protected virtual void OnPlayerEnter(ActivityUnit player)
    {
        this.player = player.GetComponent<T1>();
        controller.OnUnitSelected += OnUnitSelected;
    }

    protected virtual void OnPlayerExit(ActivityUnit player)
    {
        this.player = null;
        controller.OnUnitSelected -= OnUnitSelected;
    }

    protected virtual void OnUnitSelected(T1 unit)
    {
    }
}
