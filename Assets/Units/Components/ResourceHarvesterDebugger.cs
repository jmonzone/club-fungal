using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Debug helper to visualize resource harvesting in the editor.
/// Attach this to a UnitController to see when it's on harvest terrain.
/// </summary>
public class ResourceHarvesterDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color onTerrainColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color offTerrainColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float gizmoRadius = 1f;
    [SerializeField] private bool showDebugText = true;

    private UnitController unitController;
    private ResourceHarvesterComponentInstance harvester;

    private void Awake()
    {
        unitController = GetComponent<UnitController>();
    }

    private void Start()
    {
        if (unitController != null)
        {
            // Try to get harvester after initialization
            harvester = unitController.GetComponentInstance<ResourceHarvesterComponentInstance>();

            if (harvester != null)
            {
                harvester.OnResourceHarvested += OnResourceHarvested;
                harvester.OnEnteredHarvestTerrain += OnEnteredHarvestTerrain;
                harvester.OnExitedHarvestTerrain += OnExitedHarvestTerrain;
            }
        }
    }

    private void OnResourceHarvested(Item item, int amount)
    {
        if (showDebugText)
        {
            Debug.Log($"[{unitController.name}] Harvested {amount}x {item.Name} (Total: {harvester.TotalHarvested})");
        }
    }

    private void OnEnteredHarvestTerrain()
    {
        if (showDebugText)
        {
            Debug.Log($"[{unitController.name}] Entered harvest terrain");
        }
    }

    private void OnExitedHarvestTerrain()
    {
        if (showDebugText)
        {
            Debug.Log($"[{unitController.name}] Exited harvest terrain");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        if (unitController == null) unitController = GetComponent<UnitController>();
        if (harvester == null && unitController != null)
        {
            harvester = unitController.GetComponentInstance<ResourceHarvesterComponentInstance>();
        }

        if (harvester != null)
        {
            // Draw a sphere around the unit colored by harvest state
            Gizmos.color = harvester.IsOnHarvestTerrain ? onTerrainColor : offTerrainColor;
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, gizmoRadius);

            // Draw a wire sphere for the outline
            Gizmos.color = harvester.IsOnHarvestTerrain ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, gizmoRadius);

            // Draw info in scene view
            if (harvester.IsOnHarvestTerrain)
            {
                Handles.Label(
                    transform.position + Vector3.up * 2f,
                    $"Harvesting ({harvester.TotalHarvested} collected)",
                    new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.green } }
                );
            }
        }
    }
#endif

    private void OnDestroy()
    {
        if (harvester != null)
        {
            harvester.OnResourceHarvested -= OnResourceHarvested;
            harvester.OnEnteredHarvestTerrain -= OnEnteredHarvestTerrain;
            harvester.OnExitedHarvestTerrain -= OnExitedHarvestTerrain;
        }
    }
}
