using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PlayerService", menuName = "Club Fungal/Player/Player Service")]
public class PlayerService : GURUService
{
    [Header("Settings")]
    [SerializeField] private float speed = 2f;

    [Header("Editor Settings")]
    [SerializeField] private float editorSpeed = 5f;

    [Header("References")]
    [SerializeField] private CameraService cameraService;


    [Header("Runtime")]
    [SerializeField] private UnitInstance playerInstance;
    [SerializeField] private PlayerController player;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private IInteractable targetInteractable;

    private ActivityUnit activityUnit;

    public float Speed => Application.isEditor ? editorSpeed : speed;

    public UnitInstance PlayerInstance => playerInstance;
    public PlayerController Player => player;
    public ActivityUnit ActivityUnit => activityUnit;
    public Vector3 TargetPosition => targetPosition;
    public IInteractable TargetInteractable => targetInteractable;

    public event UnityAction OnTargetInteractableChanged;
    public event UnityAction OnTargetPositionChanged;
    public event UnityAction<bool> OnPOVCameraToggled;
    public event UnityAction OnPlayerInitialized;

    override protected void OnInitialize()
    {
        FindPlayerController();
    }

    private void OnEnable()
    {
        cameraService.OnCameraModeChanged += OnCameraModeChanged;
    }

    private void OnDisable()
    {
        cameraService.OnCameraModeChanged -= OnCameraModeChanged;
    }

    override public void OnSceneLoaded()
    {
        base.OnSceneLoaded();
        FindPlayerController();
    }

    private void FindPlayerController()
    {
        // Debug.Log("Searching for PlayerController in scene...");
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            this.player = player;
            playerInstance = player.Instance;
            activityUnit = player.GetComponent<ActivityUnit>();
            OnPlayerInitialized?.Invoke();
        }
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        SetTargetInteractable(null);

        OnTargetPositionChanged?.Invoke();
    }

    public void SetTargetInteractable(IInteractable targetInteractable)
    {
        this.targetInteractable = targetInteractable;
        OnTargetInteractableChanged?.Invoke();
    }

    private void OnCameraModeChanged(CameraMode mode)
    {
        player.RenderRoot.gameObject.SetActive(mode == CameraMode.THIRD_PERSON);
    }
}
