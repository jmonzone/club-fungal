using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component that displays cooking progress for assigned workers.
/// Shows slider progress and updates in real-time.
/// </summary>
public class CookingActionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TextMeshProUGUI progressText;

    private UnitController buildingController;
    private CookingComponentInstance cookingComponent;

    private void Awake()
    {
        if (progressSlider == null)
        {
            progressSlider = GetComponentInChildren<Slider>();
        }
        if (progressText == null)
        {
            progressText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        // Find the building controller directly
        buildingController = GetComponentInParent<UnitController>();

        if (buildingController == null)
        {
            Debug.LogWarning($"CookingActionUI: Could not find building controller for {gameObject.name}");
        }
    }

    private void Update()
    {
        if (buildingController == null) return;

        // Get the assigned worker from the building's AssignableStation
        var station = buildingController.GetComponent<AssignableStation>();
        if (station == null || station.AssignedUnit == null)
        {
            // No worker assigned - hide progress
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(false);
            }
            return;
        }

        // Get the worker's cooking component
        if (cookingComponent == null || cookingComponent.Controller != station.AssignedUnit)
        {
            cookingComponent = station.AssignedUnit.GetComponentInstance<CookingComponentInstance>();
        }

        // Update progress slider
        if (cookingComponent != null && progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = cookingComponent.ProgressPercent;

            // Update progress text if available
            if (progressText != null)
            {
                if (cookingComponent.IsCooking && cookingComponent.CurrentRecipe != null)
                {
                    progressText.text = $"Cooking {cookingComponent.CurrentRecipe.OutputItem.Name}...";
                }
                else
                {
                    progressText.text = "Idle";
                }
            }
        }
    }
}
