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

    [Header("Recipe Display")]
    [SerializeField] private Image inputItemIcon;
    [SerializeField] private TextMeshProUGUI inputItemText;
    [SerializeField] private Image outputItemIcon;
    [SerializeField] private TextMeshProUGUI outputItemText;
    [SerializeField] private GameObject recipeDisplay;

    private UnitController buildingController;
    private CookingComponentInstance cookingComponent;
    private CookingRecipe lastDisplayedRecipe;

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
            if (recipeDisplay != null)
            {
                recipeDisplay.SetActive(false);
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
                else if (cookingComponent.CurrentRecipe != null && !cookingComponent.IsCooking)
                {
                    // Has a recipe but not cooking - missing ingredients
                    progressText.text = $"Missing Ingredients";
                }
                else
                {
                    progressText.text = "Idle";
                }
            }

            // Update recipe display
            UpdateRecipeDisplay(cookingComponent.CurrentRecipe);
        }
    }

    /// <summary>
    /// Updates the recipe display to show input and output items.
    /// </summary>
    private void UpdateRecipeDisplay(CookingRecipe recipe)
    {
        // Only update if recipe changed
        if (recipe == lastDisplayedRecipe) return;
        lastDisplayedRecipe = recipe;

        if (recipeDisplay != null)
        {
            recipeDisplay.SetActive(recipe != null);
        }

        if (recipe == null) return;

        // Update input item display
        if (inputItemIcon != null && recipe.InputItem != null)
        {
            if (recipe.InputItem.Sprite != null)
            {
                inputItemIcon.sprite = recipe.InputItem.Sprite;
            }
            inputItemIcon.enabled = true;
        }

        if (inputItemText != null && recipe.InputItem != null)
        {
            inputItemText.text = recipe.InputAmount > 1
                ? $"{recipe.InputAmount}x {recipe.InputItem.Name}"
                : recipe.InputItem.Name;
        }

        // Update output item display
        if (outputItemIcon != null && recipe.OutputItem != null)
        {
            if (recipe.OutputItem.Sprite != null)
            {
                outputItemIcon.sprite = recipe.OutputItem.Sprite;
            }
            outputItemIcon.enabled = true;
        }

        if (outputItemText != null && recipe.OutputItem != null)
        {
            outputItemText.text = recipe.OutputAmount > 1
                ? $"{recipe.OutputAmount}x {recipe.OutputItem.Name}"
                : recipe.OutputItem.Name;
        }
    }
}
