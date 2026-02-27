using UnityEngine;

/// <summary>
/// DEPRECATED: Use ProximityActionComponent with RecruitableUnitProximityAction ScriptableObject instead.
/// 
/// Old example showing how to use ProximityActionComponent for unit recruitment.
/// This MonoBehaviour approach has been replaced with the ScriptableObject pattern.
/// </summary>
[System.Obsolete("Use ProximityActionComponent with RecruitableUnitProximityAction ScriptableObject instead")]
public class RecruitableUnit : MonoBehaviour
{
    [SerializeField] private UnitController unitController;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private PartyInstanceService partyService;

    private void Start()
    {
        if (unitController == null)
        {
            unitController = GetComponent<UnitController>();
        }

        // Find the proximity action component
        var proximityComponent = unitController.GetComponentInstance<ProximityActionComponentInstance>();

        if (proximityComponent == null)
        {
            Debug.LogWarning($"RecruitableUnit: No ProximityActionComponent found on {unitController.name}");
            return;
        }

        // Subscribe to the action event
        proximityComponent.OnActionTriggered += OnRecruit;
    }

    private void OnRecruit()
    {
        Debug.Log($"Recruited {unitController.name}!");

        // Change unit's Instance data to not be marked as enemy
        if (unitController.Instance != null)
        {
            // This would require adding a setter in UnitInstance or UnitData
            // For now, the visual change happens via SetAsEnemy(false)
        }

        // Add to player's party (optional)
        if (partyService != null && unitController.Instance != null)
        {
            // partyService.AddUnit(unitController.Instance);
        }

        // Disable this recruitment script
        enabled = false;

        // Optional: Play recruitment VFX/SFX
        // Optional: Show confirmation UI
    }

    private void OnDestroy()
    {
        // Clean up subscription
        var proximityComponent = unitController?.GetComponentInstance<ProximityActionComponentInstance>();
        if (proximityComponent != null)
        {
            proximityComponent.OnActionTriggered -= OnRecruit;
        }
    }
}
