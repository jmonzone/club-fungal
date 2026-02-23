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
    [SerializeField] private string buttonText = "Interact";
    [SerializeField] private Item costItem;
    [SerializeField] private int costAmount = 0;

    [Header("UI Prefab")]
    [SerializeField] private GameObject buttonPrefab;

    public PlayerService PlayerService => playerService;
    public InventoryReference GlobalInventory => globalInventory;
    public float ProximityDistance => proximityDistance;
    public bool RequireLineOfSight => requireLineOfSight;
    public string ButtonText => buttonText;
    public Item CostItem => costItem;
    public int CostAmount => costAmount;
    public GameObject ButtonPrefab => buttonPrefab;

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
    private ButtonUI buttonComponent;
    private bool isPlayerNearby;
    private bool isButtonShown;

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
        buttonComponent = buttonUI.GetComponent<ButtonUI>();

        if (buttonComponent == null)
        {
            Debug.LogWarning($"ProximityActionComponent: Button prefab missing ButtonUI component");
            Object.Destroy(buttonUI);
            return;
        }

        // Build button text
        string displayText = ProximityDefinition.ButtonText;
        if (ProximityDefinition.CostItem != null && ProximityDefinition.CostAmount > 0)
        {
            displayText = $"{ProximityDefinition.ButtonText} for {ProximityDefinition.CostAmount} {ProximityDefinition.CostItem.Name}";
        }

        buttonComponent.Initialize(displayText, OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        // Check if player can afford the cost
        if (ProximityDefinition.CostItem != null && ProximityDefinition.CostAmount > 0)
        {
            if (ProximityDefinition.GlobalInventory == null)
            {
                Debug.LogWarning("ProximityActionComponent: GlobalInventory not found");
                return;
            }

            int currentAmount = ProximityDefinition.GlobalInventory.GetItemCount(ProximityDefinition.CostItem);
            if (currentAmount < ProximityDefinition.CostAmount)
            {
                Debug.Log($"ProximityActionComponent: Not enough {ProximityDefinition.CostItem.Name}. Need {ProximityDefinition.CostAmount}, have {currentAmount}");
                return;
            }

            // Deduct cost
            ProximityDefinition.GlobalInventory.RemoveItem(ProximityDefinition.CostItem, ProximityDefinition.CostAmount);
        }

        // Trigger action event
        OnActionTriggered?.Invoke();

        // Hide button after action
        HideButton();
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
