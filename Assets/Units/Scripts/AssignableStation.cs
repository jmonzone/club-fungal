using UnityEngine;

/// <summary>
/// MonoBehaviour component that marks a building as assignable to workers.
/// Specifies what component to apply to assigned workers and where they should stand.
/// </summary>
public class AssignableStation : MonoBehaviour
{
    [Header("Assignment Configuration")]
    [SerializeField] private UnitComponentDefinition workerComponent;
    [SerializeField] private Transform assignmentTransform;

    public UnitComponentDefinition WorkerComponent => workerComponent;
    public Transform AssignmentTransform => assignmentTransform != null ? assignmentTransform : transform;

    private void Start()
    {
        // For static buildings without a UnitInstance, manually initialize components
        // This ensures proximity actions and other components work on scene-placed buildings
        var controller = GetComponent<UnitController>();
        if (controller != null && (controller.Instance == null || string.IsNullOrEmpty(controller.Instance.Id)) && controller.ComponentInstances.Count == 0)
        {
            controller.InitializeComponents();
        }
    }
}
