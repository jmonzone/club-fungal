using System.Collections;
using UnityEngine;

public class AquariumInventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private InventoryReference inventoryReference;

    [Header("Settings")]
    [SerializeField] private float speedBoostMultiplier = 3f;
    [SerializeField] private float speedBoostDuration = 3f;

    private void OnEnable()
    {
        inventoryUI.OnItemSelected += HandleItemSelected;
    }

    private void OnDisable()
    {
        inventoryUI.OnItemSelected -= HandleItemSelected;
    }

    private void HandleItemSelected(Item item)
    {
        Debug.Log("speed boost");
        StartCoroutine(ApplySpeedBoostToParty(item));
    }

    private IEnumerator ApplySpeedBoostToParty(Item item)
    {
        if (networkRunService == null || networkRunService.PartyControllers == null)
        {
            Debug.LogWarning("NetworkRunService or PartyControllers not available");
            yield break;
        }

        foreach (var controller in networkRunService.PartyControllers)
        {
            var speedModifier = controller.GetComponent<UnitSpeedModifier>();
            if (speedModifier == null)
            {
                speedModifier = controller.gameObject.AddComponent<UnitSpeedModifier>();
            }
            speedModifier.AddSpeedModifier(speedBoostMultiplier);

            // Activate trail effect
            var trail = controller.GetComponent<UnitTrail>();
            if (trail == null)
            {
                trail = controller.gameObject.AddComponent<UnitTrail>();
            }
            trail.ActivateTrail();
        }

        Debug.Log($"Applied {speedBoostMultiplier}x speed boost to {networkRunService.PartyControllers.Count} party members for {speedBoostDuration} seconds");

        // Remove item from inventory
        if (inventoryReference != null)
        {
            inventoryReference.RemoveItem(item, 1);
        }

        yield return new WaitForSeconds(speedBoostDuration);

        foreach (var controller in networkRunService.PartyControllers)
        {
            var speedModifier = controller.GetComponent<UnitSpeedModifier>();
            if (speedModifier != null)
            {
                speedModifier.RemoveSpeedModifier(speedBoostMultiplier);
            }

            // Deactivate trail effect
            var trail = controller.GetComponent<UnitTrail>();
            if (trail != null)
            {
                trail.DeactivateTrail();
            }
        }

        Debug.Log("Speed boost expired");
    }
}
