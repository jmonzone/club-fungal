using UnityEngine;

/// <summary>
/// Automatically collects nearby collectible units.
/// </summary>
[CreateAssetMenu(fileName = "CollectorComponent", menuName = "Club Fungal/Units/Components/Collector Component")]
public class CollectorComponentDefinition : UnitComponentDefinition
{
    [Header("Collection Settings")]
    [SerializeField] private float collectionRadius = 0.5f;
    [SerializeField] private LayerMask collectibleLayers = -1;

    public float CollectionRadius => collectionRadius;
    public LayerMask CollectibleLayers => collectibleLayers;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new CollectorComponentInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of collector component.
/// Manages detection and collection of nearby collectibles.
/// </summary>
[System.Serializable]
public class CollectorComponentInstance : UnitComponentInstance
{
    public CollectorComponentDefinition CollectorDefinition => definition as CollectorComponentDefinition;

    public CollectorComponentInstance(CollectorComponentDefinition definition, UnitController controller)
        : base(definition, controller)
    {
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // Find nearby collectibles
        var colliders = Physics.OverlapSphere(
            controller.transform.position,
            CollectorDefinition.CollectionRadius,
            CollectorDefinition.CollectibleLayers
        );

        foreach (var collider in colliders)
        {
            var unitController = collider.GetComponentInParent<UnitController>();
            if (unitController == null || unitController == controller) continue;

            var collectible = unitController.GetComponentInstance<CollectibleComponentInstance>();
            if (collectible != null)
            {
                collectible.TryCollect(controller);
            }
        }
    }
}
