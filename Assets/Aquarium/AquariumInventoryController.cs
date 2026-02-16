using System.Collections;
using UnityEngine;

public class AquariumInventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private NetworkRunService networkRunService;

    [Header("Settings")]
    [SerializeField] private float speedBoostMultiplier = 2f;
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
        StartCoroutine(ApplySpeedBoostToParty());
    }

    private IEnumerator ApplySpeedBoostToParty()
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
        }

        Debug.Log($"Applied {speedBoostMultiplier}x speed boost to {networkRunService.PartyControllers.Count} party members for {speedBoostDuration} seconds");

        yield return new WaitForSeconds(speedBoostDuration);

        foreach (var controller in networkRunService.PartyControllers)
        {
            var speedModifier = controller.GetComponent<UnitSpeedModifier>();
            if (speedModifier != null)
            {
                speedModifier.RemoveSpeedModifier(speedBoostMultiplier);
            }
        }

        Debug.Log("Speed boost expired");
    }
}
