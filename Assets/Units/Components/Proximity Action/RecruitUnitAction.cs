using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Action for recruiting neutral units to the player's party.
/// Can require item costs and adds unit to the party.
/// </summary>
[CreateAssetMenu(fileName = "RecruitUnitAction", menuName = "Club Fungal/Unit Actions/Recruit Unit")]
public class RecruitUnitAction : UnitAction
{
    [Header("Services")]
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private NetworkRunPartyService partyService;
    [SerializeField] private InventoryReference globalInventory;

    [Header("Cost Settings")]
    [SerializeField] private List<Item> costItems = new List<Item>();
    [SerializeField] private int minCostAmount = 1;
    [SerializeField] private int maxCostAmount = 3;

    [Header("Settings")]
    [SerializeField] private bool addToParty = true;

    public override void Execute(UnitController controller)
    {
        if (controller == null)
        {
            Debug.LogWarning("RecruitUnitAction: Controller is null");
            return;
        }

        // Check and deduct cost if required
        if (costItems != null && costItems.Count > 0)
        {
            if (globalInventory == null)
            {
                Debug.LogWarning("RecruitUnitAction: GlobalInventory not assigned but cost items are required");
                return;
            }

            // Select random cost item and amount
            Item selectedItem = costItems[Random.Range(0, costItems.Count)];
            int requiredAmount = Random.Range(minCostAmount, maxCostAmount + 1);

            int currentAmount = globalInventory.GetItemCount(selectedItem);
            if (currentAmount < requiredAmount)
            {
                Debug.Log($"RecruitUnitAction: Not enough {selectedItem.Name}. Need {requiredAmount}, have {currentAmount}");
                return;
            }

            // Deduct cost
            globalInventory.RemoveItem(selectedItem, requiredAmount);
            Debug.Log($"Spent {requiredAmount}x {selectedItem.Name} to recruit {controller.Instance?.DisplayName}");
        }

        Debug.Log($"Recruited {controller.name}!");

        // Register unit so it will be persisted
        if (controller.Instance != null && unitInstanceService != null)
        {
            if (!unitInstanceService.Instances.Contains(controller.Instance))
            {
                unitInstanceService.RegisterUnit(controller.Instance);
            }
        }

        // Add to player's party
        if (addToParty && partyService != null && controller.Instance != null)
        {
            if (!partyService.PartyControllers.Contains(controller))
            {
                partyService.AddUnitToParty(controller);
                Debug.Log($"Added {controller.Instance.DisplayName} to party");
            }
        }

        // Save to local data so recruited unit persists
        if (unitInstanceService != null)
        {
            unitInstanceService.SaveData();
            Debug.Log($"Saved recruited unit {controller.Instance?.DisplayName} to local data");
        }

        // Optional: Play recruitment VFX/SFX here
        // Optional: Show confirmation UI here
    }
}

