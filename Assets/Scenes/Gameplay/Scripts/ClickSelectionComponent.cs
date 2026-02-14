using UnityEngine;
using UnityEngine.Events;

public class ClickSelectionComponent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float raycastMaxDistance = 100f;

    public UnityEvent<IInteractable> OnInteractableClicked;
    public UnityEvent<Vector3> OnGroundClicked;

    private bool isDragging = false;
    private Vector3 startInput;

    private void Awake()
    {
        if (!mainCamera) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startInput = Input.mousePosition;
            isDragging = false;
        }

        if (Input.GetMouseButton(0))
        {
            var inputDelta = Input.mousePosition - startInput;
            if (inputDelta.magnitude > 0.1f) isDragging = true;
        }

        if (Input.GetMouseButtonUp(0) && !isDragging)
        {
            Vector3 inputPos = Input.mousePosition;
            Ray ray = mainCamera.ScreenPointToRay(inputPos);
            RaycastHit hit;

            if (Physics.SphereCast(ray, 0.001f, out hit, 1000f, interactableMask))
            {
                var interactable = hit.transform.GetComponentInParent<IInteractable>();
                if (interactable != null)
                {
                    OnInteractableClicked?.Invoke(interactable);
                    return;
                }
            }

            if (Physics.Raycast(ray, out hit, raycastMaxDistance, groundMask))
            {
                OnGroundClicked?.Invoke(hit.point);
            }
        }
    }
}
