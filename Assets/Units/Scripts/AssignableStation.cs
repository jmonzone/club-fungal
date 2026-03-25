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

    [Header("Proximity Action Components")]
    [Tooltip("ProximityActionComponent to use when no unit is assigned (shows assignment UI)")]
    [SerializeField] private ProximityActionComponentDefinition unassignedProximityAction;
    [Tooltip("ProximityActionComponent to use when unit is assigned (shows worker info/progress)")]
    [SerializeField] private ProximityActionComponentDefinition assignedProximityAction;

    private UnitController assignedUnit;

    public UnitComponentDefinition WorkerComponent => workerComponent;
    public Transform AssignmentTransform => assignmentTransform != null ? assignmentTransform : transform;
    public ProximityActionComponentDefinition UnassignedProximityAction => unassignedProximityAction;
    public ProximityActionComponentDefinition AssignedProximityAction => assignedProximityAction;
    public UnitController AssignedUnit => assignedUnit;

    public void SetAssignedUnit(UnitController unit)
    {
        assignedUnit = unit;
    }

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
