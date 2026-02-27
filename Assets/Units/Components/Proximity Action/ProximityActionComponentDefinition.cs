using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ScriptableObject definition for a proximity action component.
/// Shows an overhead UI button when player is nearby and allows customizable actions.
/// </summary>
[CreateAssetMenu(fileName = "ProximityActionComponent", menuName = "Club Fungal/Units/Components/Proximity Action Component")]
public class ProximityActionComponentDefinition : UnitComponentDefinition
{
    [Header("Service References")]
    [SerializeField] private PlayerService playerService;
    [SerializeField] private InventoryReference globalInventory;

    [Header("Proximity Settings")]
    [SerializeField] private float proximityDistance = 3f;
    [SerializeField] private bool requireLineOfSight = false;

    [Header("Button Settings")]
    [SerializeField] private List<Item> costItems = new List<Item>();
    [SerializeField] private int minCostAmount = 1;
    [SerializeField] private int maxCostAmount = 3;

    [Header("Action")]
    [SerializeField] private ProximityAction action;

    [Header("UI Prefab")]
    [SerializeField] private GameObject buttonPrefab;

    public PlayerService PlayerService => playerService;
    public InventoryReference GlobalInventory => globalInventory;
    public float ProximityDistance => proximityDistance;
    public bool RequireLineOfSight => requireLineOfSight;
    public List<Item> CostItems => costItems;
    public int MinCostAmount => minCostAmount;
    public int MaxCostAmount => maxCostAmount;
    public GameObject ButtonPrefab => buttonPrefab;
    public ProximityAction Action => action;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new ProximityActionComponentInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of proximity action component.
/// Manages player proximity detection and UI button display.
/// </summary>
[System.Serializable]
public class ProximityActionComponentInstance : UnitComponentInstance
{
    private GameObject buttonUI;
    private ProximityActionUI proximityActionUI;
    private bool isPlayerNearby;
    private bool isButtonShown;
    private Item selectedCostItem;
    private int selectedCostAmount;

    public ProximityActionComponentDefinition ProximityDefinition => definition as ProximityActionComponentDefinition;

    // Event that fires when button is clicked - can be subscribed to for custom behavior
    public event UnityAction OnActionTriggered;

    public ProximityActionComponentInstance(ProximityActionComponentDefinition definition, UnitController controller)
        : base(definition, controller)
    {
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        // Select random cost item and amount
        if (ProximityDefinition.CostItems != null && ProximityDefinition.CostItems.Count > 0)
        {
            selectedCostItem = ProximityDefinition.CostItems[Random.Range(0, ProximityDefinition.CostItems.Count)];
            selectedCostAmount = Random.Range(ProximityDefinition.MinCostAmount, ProximityDefinition.MaxCostAmount + 1);
        }

        CreateButtonUI();
        HideButton();
    }

    private void CreateButtonUI()
    {
        if (ProximityDefinition.ButtonPrefab == null)
        {
            Debug.LogWarning($"ProximityActionComponent: No button prefab assigned for {controller.name}");
            return;
        }

        if (controller.OverheadCanvasPosition == null || controller.OverheadCanvasPosition.TargetContainer == null)
        {
            Debug.LogWarning($"ProximityActionComponent: Unit missing OverheadCanvasPosition or TargetContainer for {controller.name}");
            return;
        }

        // Instantiate button UI in the overhead canvas target container
        buttonUI = Object.Instantiate(ProximityDefinition.ButtonPrefab, controller.OverheadCanvasPosition.TargetContainer);
        proximityActionUI = buttonUI.GetComponent<ProximityActionUI>();

        if (proximityActionUI == null)
        {
            Debug.LogWarning($"ProximityActionComponent: Button prefab missing ProximityActionUI component");
            Object.Destroy(buttonUI);
            return;
        }

        // Initialize with item icon and click handler
        Sprite icon = selectedCostItem?.Sprite;
        proximityActionUI.Initialize("Recruit", icon, OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        // Check if player can afford the cost
        if (selectedCostItem != null && selectedCostAmount > 0)
        {
            if (ProximityDefinition.GlobalInventory == null)
            {
                Debug.LogWarning("ProximityActionComponent: GlobalInventory not found");
                return;
            }

            int currentAmount = ProximityDefinition.GlobalInventory.GetItemCount(selectedCostItem);
            if (currentAmount < selectedCostAmount)
            {
                Debug.Log($"ProximityActionComponent: Not enough {selectedCostItem.Name}. Need {selectedCostAmount}, have {currentAmount}");
                return;
            }

            // Deduct cost
            ProximityDefinition.GlobalInventory.RemoveItem(selectedCostItem, selectedCostAmount);
        }

        // Execute the action
        if (ProximityDefinition.Action != null)
        {
            ProximityDefinition.Action.Execute(controller);
        }

        // Trigger action event (for custom behavior)
        OnActionTriggered?.Invoke();

        // Remove this component from the unit controller
        controller.RemoveComponent(this);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        if (ProximityDefinition.PlayerService == null || ProximityDefinition.PlayerService.Player == null)
        {
            if (isButtonShown)
            {
                HideButton();
            }
            return;
        }

        // Check distance to player
        float distance = Vector3.Distance(controller.transform.position, ProximityDefinition.PlayerService.Player.transform.position);
        isPlayerNearby = distance <= ProximityDefinition.ProximityDistance;

        // Check line of sight if required
        if (isPlayerNearby && ProximityDefinition.RequireLineOfSight)
        {
            Vector3 direction = ProximityDefinition.PlayerService.Player.transform.position - controller.transform.position;
            if (Physics.Raycast(controller.transform.position, direction, distance))
            {
                isPlayerNearby = false;
            }
        }

        // Show/hide button based on proximity
        if (isPlayerNearby && !isButtonShown)
        {
            ShowButton();
        }
        else if (!isPlayerNearby && isButtonShown)
        {
            HideButton();
        }

        // Update progress UI if button is shown
        if (isButtonShown)
        {
            UpdateProgressUI();
        }
    }

    private void UpdateProgressUI()
    {
        if (selectedCostItem == null || selectedCostAmount <= 0) return;
        if (proximityActionUI == null) return;

        int currentAmount = ProximityDefinition.GlobalInventory?.GetItemCount(selectedCostItem) ?? 0;
        int requiredAmount = selectedCostAmount;

        proximityActionUI.UpdateProgress(currentAmount, requiredAmount);
    }

    private void ShowButton()
    {
        if (buttonUI != null)
        {
            buttonUI.SetActive(true);
            isButtonShown = true;
        }
    }

    private void HideButton()
    {
        if (buttonUI != null)
        {
            buttonUI.SetActive(false);
            isButtonShown = false;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        if (buttonUI != null)
        {
            Object.Destroy(buttonUI);
        }
    }
}
