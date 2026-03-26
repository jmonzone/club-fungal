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

    [Header("Proximity Settings")]
    [SerializeField] private float proximityDistance = 3f;

    [Header("Button Settings")]
    [SerializeField] private string buttonText = "Interact";

    [Header("Action")]
    [SerializeField] private UnitAction action;

    [Header("UI Prefab")]
    [SerializeField] private GameObject buttonPrefab;

    public PlayerService PlayerService => playerService;
    public float ProximityDistance => proximityDistance;
    public string ButtonText => buttonText;
    public GameObject ButtonPrefab => buttonPrefab;
    public UnitAction Action => action;

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
        proximityActionUI = buttonUI.GetComponent<ProximityActionUI>();

        if (proximityActionUI == null)
        {
            Debug.LogWarning($"ProximityActionComponent: Button prefab missing ProximityActionUI component");
            Object.Destroy(buttonUI);
            return;
        }

        // Initialize button with text and click handler
        string buttonText = ProximityDefinition.ButtonText ?? "Interact";
        proximityActionUI.Initialize(buttonText, OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        // Execute the action
        if (ProximityDefinition.Action != null)
        {
            ProximityDefinition.Action.Execute(controller);
        }

        // Trigger action event (for custom behavior)
        OnActionTriggered?.Invoke();

        // Only remove component for one-time actions (like recruit)
        // Assignment and selection actions persist to keep showing UI
        bool isPersistentAction = ProximityDefinition.Action is AssignUnitAction ||
                                   ProximityDefinition.Action is SelectUnitAction;
        if (!isPersistentAction)
        {
            controller.RemoveComponent(this);
        }
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
