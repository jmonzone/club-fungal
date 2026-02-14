using UnityEngine;

public class DirectInteractableSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ClickSelectionComponent clickSelection;
    [SerializeField] private Transform selected;

    private void Awake()
    {
        if (!clickSelection) clickSelection = GetComponent<ClickSelectionComponent>();
        clickSelection.OnInteractableClicked.AddListener(SelectInteractable);
    }

    private void SelectInteractable(IInteractable interactable)
    {
        selected = interactable.Transform;
        interactable.Select(null); // Pass null or the appropriate UnitController if needed
        // Optionally, add any additional logic for selection feedback here
    }
}
