using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

/// <summary>
/// ScriptableObject definition for a resource harvester component.
/// Collects resources when unit is standing on specific NavMesh terrain areas.
/// </summary>
[CreateAssetMenu(fileName = "ResourceHarvesterComponent", menuName = "Club Fungal/Units/Components/Resource Harvester Component")]
public class ResourceHarvesterComponentDefinition : UnitComponentDefinition
{
    [Header("NavMesh Settings")]
    [NavMeshArea]
    [Tooltip("The NavMesh area to harvest from - select from dropdown")]
    [SerializeField] private int harvestAreaIndex = 6;

    [Header("Resource Settings")]
    [Tooltip("The item to collect when standing on the harvest area")]
    [SerializeField] private Item resourceItem;

    [Tooltip("How many items to collect per harvest")]
    [SerializeField] private int amountPerHarvest = 1;

    [Tooltip("Time in seconds between each harvest")]
    [SerializeField] private float harvestInterval = 2f;

    [Tooltip("Minimum time in seconds the unit must remain on harvest terrain before collecting")]
    [SerializeField] private float minimumStayDuration = 0.5f;

    [Header("Service Reference")]
    [Tooltip("Service to notify when resources are harvested")]
    [SerializeField] private ResourceHarvestService resourceHarvestService;

    public int HarvestAreaIndex => harvestAreaIndex;
    public Item ResourceItem => resourceItem;
    public int AmountPerHarvest => amountPerHarvest;
    public float HarvestInterval => harvestInterval;
    public float MinimumStayDuration => minimumStayDuration;
    public ResourceHarvestService ResourceHarvestService => resourceHarvestService;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new ResourceHarvesterComponentInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of resource harvester component.
/// Monitors unit position and collects resources when on harvest terrain.
/// </summary>
[System.Serializable]
public class ResourceHarvesterComponentInstance : UnitComponentInstance
{
    [SerializeField] private bool isOnHarvestTerrain;
    [SerializeField] private float timeOnTerrain;
    [SerializeField] private float timeSinceLastHarvest;
    private NavMeshAgent navMeshAgent;

    public ResourceHarvesterComponentDefinition HarvesterDefinition => definition as ResourceHarvesterComponentDefinition;

    public event UnityAction<Item, int> OnResourceHarvested;
    public event UnityAction OnEnteredHarvestTerrain;
    public event UnityAction OnExitedHarvestTerrain;

    public ResourceHarvesterComponentInstance(ResourceHarvesterComponentDefinition definition, UnitController controller)
        : base(definition, controller)
    {
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        navMeshAgent = controller.GetComponent<NavMeshAgent>();
        ResetHarvestState();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (HarvesterDefinition.ResourceItem == null) return;

        bool wasOnTerrain = isOnHarvestTerrain;
        isOnHarvestTerrain = CheckIfOnHarvestTerrain();

        // Handle terrain state changes
        if (isOnHarvestTerrain && !wasOnTerrain)
        {
            OnEnteredHarvestTerrain?.Invoke();
            timeOnTerrain = 0f;
            timeSinceLastHarvest = 0f;
        }
        else if (!isOnHarvestTerrain && wasOnTerrain)
        {
            OnExitedHarvestTerrain?.Invoke();
            timeOnTerrain = 0f;
        }

        // Update timers and harvest if on terrain
        if (isOnHarvestTerrain)
        {
            timeOnTerrain += Time.deltaTime;
            timeSinceLastHarvest += Time.deltaTime;

            // Check if we've been on terrain long enough and harvest interval has passed
            if (timeOnTerrain >= HarvesterDefinition.MinimumStayDuration &&
                timeSinceLastHarvest >= HarvesterDefinition.HarvestInterval)
            {
                HarvestResource();
                timeSinceLastHarvest = 0f;
            }
        }
    }

    private bool CheckIfOnHarvestTerrain()
    {
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
            return false;

        // Sample the NavMesh at the unit's current position
        if (NavMesh.SamplePosition(controller.transform.position, out NavMeshHit hit, 0.5f, NavMesh.AllAreas))
        {
            // Check if we're on the harvest area
            int harvestMask = 1 << HarvesterDefinition.HarvestAreaIndex;
            return (hit.mask & harvestMask) != 0;
        }

        return false;
    }

    private void HarvestResource()
    {
        var item = HarvesterDefinition.ResourceItem;
        var amount = HarvesterDefinition.AmountPerHarvest;
        var service = HarvesterDefinition.ResourceHarvestService;

        if (service != null)
        {
            service.Harvest(item, amount, controller.transform.position + Vector3.up);
            OnResourceHarvested?.Invoke(item, amount);
        }
        else
        {
            Debug.LogWarning($"ResourceHarvestService not assigned to {controller.name}'s harvester component");
        }
    }

    private void ResetHarvestState()
    {
        isOnHarvestTerrain = false;
        timeOnTerrain = 0f;
        timeSinceLastHarvest = 0f;
    }
}
