using UnityEngine;
using UnityEngine.UI;

public abstract class ActivityUI : MonoBehaviour
{
    private Camera mainCamera;

    public virtual Camera Camera => mainCamera;

    protected virtual void Awake()
    {
        mainCamera = Camera.main;
    }
}

public abstract class ActivityUI<T1, T2> : ActivityUI where T1 : ActivityBehaviour where T2: ActivityController<T1>
{
    [Header("Activity References")]
    [SerializeField] private PlayerActivityReference playerActivityReference;
    [SerializeField] private ActivityReference activity;
    [SerializeField] private PlayerReference playerReference;
    [SerializeField] private T2 controller;
    [SerializeField] private Button backButton;

    [Header("Activity Runtime")]
    [SerializeField] private T1 player;

    protected ActivityReference Activity => activity;
    protected PlayerReference PlayerReference => playerReference;
    protected T1 Player => player;
    protected T2 Controller => controller;
    protected Button BackButton => backButton;
    protected bool PlayerIsSelected => player == controller.CurrentUnit;

    protected override void Awake()
    {
        base.Awake();
        BackButton.onClick.AddListener(playerActivityReference.ExitActivity);
    }

    protected virtual void OnEnable()
    {
        activity.OnUnitEnter += OnUnitEnter;
        activity.OnUnitExit += OnUnitExit;
        activity.OnPlayerEnter += OnPlayerEnter;
        activity.OnPlayerExit += OnPlayerExit;
    }

    protected virtual void OnDisable()
    {
        activity.OnUnitEnter -= OnUnitEnter;
        activity.OnUnitExit -= OnUnitExit;
        activity.OnPlayerEnter -= OnPlayerEnter;
        activity.OnPlayerExit -= OnPlayerExit;
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
