using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PlayerReference", menuName = "Club Fungal/Player/Player Reference")]
public class PlayerReference : ScriptableObject
{
    [Header("Settings")]
    [SerializeField] private float speed = 2f;

    [Header("Editor Settings")]
    [SerializeField] private float editorSpeed = 5f;


    [Header("Runtime")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private IInteractable targetInteractable;

    private ActivityUnit activityUnit;

    public float Speed => Application.isEditor ? editorSpeed : speed;

    public PlayerController Player => player;
    public ActivityUnit ActivityUnit => activityUnit;
    public Vector3 TargetPosition => targetPosition;
    public IInteractable TargetInteractable => targetInteractable;

    public event UnityAction<PlayerController> OnPlayerSet;
    public event UnityAction OnTargetInteractableChanged;
    public event UnityAction OnTargetPositionChanged;
    public event UnityAction<bool> OnPOVCameraToggled;

    public void SetPlayer(PlayerController player)
    {
        this.player = player;
        activityUnit = player.GetComponent<ActivityUnit>();
        OnPlayerSet?.Invoke(player);
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

    public void SetCameraZoom(float t)
    {
        player.RenderRoot.gameObject.SetActive(t >= 0.01f);
    }
}
