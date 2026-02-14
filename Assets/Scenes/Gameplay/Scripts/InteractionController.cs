using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public interface ITarget
{
    public Transform Transform { get; }
}

public interface IInteractable : ITarget
{
    public void Select(UnitController source);
}

public class InteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerService playerReference;
    [SerializeField] private DialogueReference dialogue;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private ViewReference homeView;
    [SerializeField] private VirtualJoystick virtualJoystick;
    [SerializeField] private CameraRotationController cameraRotationController;
    [SerializeField] private ClickSelectionComponent clickSelectionComponent;

    [Header("Runtime")]
    [SerializeField] private Transform selected;

    private Camera mainCamera;

    // Click logic moved to ClickSelectionComponent

    public event UnityAction<Transform> OnEntitySelected;
    public event UnityAction<Vector3> OnGroundSelected;

    private void Awake()
    {
        mainCamera = Camera.main;
        dialogue.OnDialogueComplete += Unselect;

        virtualJoystick.OnJoystickStart += position =>
        {
            cameraRotationController.SetCanOrbit(false);
        };

        virtualJoystick.OnJoystickEnd += () =>
        {
            cameraRotationController.SetCanOrbit(true);
        };

        if (!clickSelectionComponent) clickSelectionComponent = GetComponent<ClickSelectionComponent>();
        if (clickSelectionComponent)
        {
            clickSelectionComponent.OnInteractableClicked.AddListener(HandleClickInteractable);
            clickSelectionComponent.OnGroundClicked.AddListener(HandleClickGround);
        }
    }

    private float raycastMaxDistance = 100f;

    private List<IInteractable> previousInteractables = new List<IInteractable>();

    private void Update()
    {
        if (!homeView.Canvas.IsVisible) return;

        var proximityColliders = Physics.OverlapSphere(playerReference.Player.transform.position, 1f, interactableMask);

        var proximityInteractables = proximityColliders
            .Select(c => c.GetComponentInParent<IInteractable>())
            .Where(i => i != null)
            .ToList();

        // Handle leaving proximity
        foreach (var interactable in previousInteractables.Except(proximityInteractables).ToList())
        {
            // interactable.OnProximityChanged(false);
            previousInteractables.Remove(interactable);
        }

        // Handle entering proximity
        foreach (var interactable in proximityInteractables.Except(previousInteractables).ToList())
        {
            // interactable.OnProximityChanged(true);
            previousInteractables.Add(interactable);
        }
        // Click logic handled by ClickSelectionComponent
    }

    private void HandleClickInteractable(IInteractable interactable)
    {
        playerReference.SetTargetInteractable(interactable);
        OnEntitySelected?.Invoke(interactable.Transform);
        selected = interactable.Transform;
    }

    private void HandleClickGround(Vector3 point)
    {
        playerReference.SetTargetPosition(point);
        OnGroundSelected?.Invoke(point);
        selected = null;
    }

    public void Unselect()
    {
        if (selected)
        {
            selected = null;
            OnEntitySelected?.Invoke(null);
        }
    }
}
