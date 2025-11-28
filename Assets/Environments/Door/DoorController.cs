using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour, IInteractable, IJoystickSelectable
{
    [Header("Runtime")]
    [SerializeField] private RoomController roomController;
    [SerializeField] private WallController wallController;
    [SerializeField] private DoorController otherDoor;

    Transform ITarget.Transform => transform;
    public WallController Wall => wallController;
    public RoomController Room => roomController;
    public DoorController OtherDoor => otherDoor;

    public event UnityAction OnDoorSelected;

    private void Awake()
    {
        wallController = GetComponentInParent<WallController>();
        roomController = GetComponentInParent<RoomController>();
    }

    public void Select(UnitController source)
    {
        OnDoorSelected?.Invoke();
    }

    public void SetOtherDoor(DoorController door)
    {
        otherDoor = door;
    }
}
