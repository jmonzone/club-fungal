using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a cooking recipe: input item converts to output item over time.
/// </summary>
[System.Serializable]
public class CookingRecipe
{
    [SerializeField] private Item inputItem;
    [SerializeField] private int inputAmount = 1;
    [SerializeField] private Item outputItem;
    [SerializeField] private int outputAmount = 1;
    [SerializeField] private float cookingTime = 5f;

    public Item InputItem => inputItem;
    public int InputAmount => inputAmount;
    public Item OutputItem => outputItem;
    public int OutputAmount => outputAmount;
    public float CookingTime => cookingTime;

    public bool CanCook(InventoryReference sourceInventory)
    {
        if (sourceInventory == null || inputItem == null) return false;
        return sourceInventory.GetItemCount(inputItem) >= inputAmount;
    }
}

/// <summary>
/// ScriptableObject definition for a cooking component.
/// Converts input items to output items over time (e.g., raw fish to cooked fish).
/// </summary>
[CreateAssetMenu(fileName = "CookingComponent", menuName = "Club Fungal/Units/Components/Cooking Component")]
public class CookingComponentDefinition : UnitComponentDefinition
{
    [Header("Recipes")]
    [Tooltip("List of recipes this cooking station can use")]
    [SerializeField] private List<CookingRecipe> recipes = new List<CookingRecipe>();

    [Header("Inventory Settings")]
    [Tooltip("Global inventory to pull input items from and put output items into")]
    [SerializeField] private InventoryReference globalInventory;

    [Tooltip("Maximum distance to search for input items (0 = unlimited)")]
    [SerializeField] private float searchRadius = 10f;

    [Header("Cooking Settings")]
    [Tooltip("Auto-select next available recipe when current finishes")]
    [SerializeField] private bool autoSelectRecipe = true;

    public List<CookingRecipe> Recipes => recipes;
    public InventoryReference GlobalInventory => globalInventory;
    public float SearchRadius => searchRadius;
    public bool AutoSelectRecipe => autoSelectRecipe;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new CookingComponentInstance(this, controller);
    }
}

/// <summary>
/// Runtime instance of cooking component.
/// Manages active recipe, cooking progress, and resource conversion.
/// </summary>
[System.Serializable]
public class CookingComponentInstance : UnitComponentInstance
{
    [SerializeField] private CookingRecipe currentRecipe;
    [SerializeField] private float cookingProgress;
    [SerializeField] private bool isCooking;

    public CookingComponentDefinition CookingDefinition => definition as CookingComponentDefinition;
    public CookingRecipe CurrentRecipe => currentRecipe;
    public float CookingProgress => cookingProgress;
    public bool IsCooking => isCooking;

    // Progress as percentage (0-1)
    public float ProgressPercent
    {
        get
        {
            if (currentRecipe == null || currentRecipe.CookingTime <= 0) return 0f;
            return Mathf.Clamp01(cookingProgress / currentRecipe.CookingTime);
        }
    }

    public CookingComponentInstance(CookingComponentDefinition definition, UnitController controller)
        : base(definition, controller)
    {
    }

    public override void OnInitialize()
    {
        base.OnInitialize();

        // Try to select a recipe if auto-select is enabled
        if (CookingDefinition.AutoSelectRecipe)
        {
            TrySelectRecipe();
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        // If no recipe selected, try to auto-select one
        if (currentRecipe == null && CookingDefinition.AutoSelectRecipe)
        {
            TrySelectRecipe();
        }

        // Update cooking if we have a recipe
        if (currentRecipe != null && isCooking)
        {
            UpdateCooking();
        }
    }

    /// <summary>
    /// Try to select the first available recipe that has ingredients.
    /// </summary>
    private void TrySelectRecipe()
    {
        if (CookingDefinition.Recipes == null || CookingDefinition.Recipes.Count == 0)
        {
            return;
        }

        var inventory = CookingDefinition.GlobalInventory;
        if (inventory == null)
        {
            return;
        }

        // Find first recipe with available ingredients and start cooking
        foreach (var recipe in CookingDefinition.Recipes)
        {
            if (recipe.CanCook(inventory))
            {
                StartCooking(recipe);
                return;
            }
        }

        // If no recipe has ingredients, select the first recipe anyway to show what's needed
        if (CookingDefinition.Recipes.Count > 0 && currentRecipe == null)
        {
            currentRecipe = CookingDefinition.Recipes[0];
            isCooking = false;
        }
    }

    /// <summary>
    /// Start cooking with the specified recipe.
    /// </summary>
    public void StartCooking(CookingRecipe recipe)
    {
        if (recipe == null)
        {
            Debug.LogWarning("CookingComponent: Cannot start cooking with null recipe");
            return;
        }

        var inventory = CookingDefinition.GlobalInventory;
        if (inventory == null)
        {
            Debug.LogWarning("CookingComponent: No inventory reference");
            return;
        }

        // Check if we have ingredients
        if (!recipe.CanCook(inventory))
        {
            Debug.Log($"CookingComponent: Not enough {recipe.InputItem.Name} to cook");
            return;
        }

        // Consume input items
        inventory.RemoveItem(recipe.InputItem, recipe.InputAmount);

        // Start cooking
        currentRecipe = recipe;
        cookingProgress = 0f;
        isCooking = true;

        Debug.Log($"Started cooking {recipe.OutputItem.Name} (takes {recipe.CookingTime}s)");
    }

    /// <summary>
    /// Update cooking progress and complete when done.
    /// </summary>
    private void UpdateCooking()
    {
        if (currentRecipe == null) return;

        // Increment progress
        cookingProgress += Time.deltaTime;

        // Check if cooking is complete
        if (cookingProgress >= currentRecipe.CookingTime)
        {
            CompleteCooking();
        }
    }

    /// <summary>
    /// Complete the current cooking recipe and produce output.
    /// </summary>
    private void CompleteCooking()
    {
        if (currentRecipe == null) return;

        var inventory = CookingDefinition.GlobalInventory;
        if (inventory == null)
        {
            Debug.LogWarning("CookingComponent: No inventory reference for output");
            return;
        }

        // Add output items to inventory
        for (int i = 0; i < currentRecipe.OutputAmount; i++)
        {
            inventory.AddItem(currentRecipe.OutputItem);
        }

        Debug.Log($"Cooking complete! Produced {currentRecipe.OutputAmount}x {currentRecipe.OutputItem.Name}");

        // Reset cooking state
        currentRecipe = null;
        cookingProgress = 0f;
        isCooking = false;

        // Try to start next recipe if auto-select is enabled
        if (CookingDefinition.AutoSelectRecipe)
        {
            TrySelectRecipe();
        }
    }

    /// <summary>
    /// Cancel current cooking (doesn't refund ingredients).
    /// </summary>
    public void CancelCooking()
    {
        currentRecipe = null;
        cookingProgress = 0f;
        isCooking = false;
    }

    /// <summary>
    /// Manually select a specific recipe to cook.
    /// </summary>
    public bool SelectRecipe(CookingRecipe recipe)
    {
        if (recipe == null) return false;

        var inventory = CookingDefinition.GlobalInventory;
        if (inventory == null || !recipe.CanCook(inventory))
        {
            return false;
        }

        StartCooking(recipe);
        return true;
    }
}
