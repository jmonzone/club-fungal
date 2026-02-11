using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CostOverride
{
    public int index; // Level/index (0-based)
    [Tooltip("Cost override. Set to -1 to use formula, or >= 0 to override.")]
    public int cost = -1;
}

/// <summary>
/// Configures how resource costs scale over time/levels for activities.
/// Used by ResourceContributionHandler for dynamic cost calculation.
/// </summary>
[CreateAssetMenu(fileName = "ResourceScaling", menuName = "Club Fungal/Activities/Resource Scaling Config")]
public class ResourceScalingConfig : ScriptableObject
{
    [Header("Base Scaling")]
    [Tooltip("Base multiplier applied to computed costs. 1.0 = normal, 0.5 = half cost, 2.0 = double cost")]
    [Range(0.1f, 10f)]
    public float costScale = 1f;

    [Tooltip("Minimum cost value (after all calculations)")]
    public int minimumCost = 1;

    [Header("Cost Overrides")]
    [Tooltip("Explicit costs for specific indices. Overrides formula calculation.")]
    public List<CostOverride> costOverrides = new List<CostOverride>();

    /// <summary>
    /// Calculate required amount using RuneScape XP formula
    /// </summary>
    public int CalculateRequiredAmount(int level)
    {
        // RuneScape XP formula: calculate total XP required for this level
        float totalXP = 0f;

        for (int lvl = 1; lvl < level; lvl++)
        {
            totalXP += Mathf.Floor(lvl + 300f * Mathf.Pow(2f, lvl / 7f));
        }

        totalXP = Mathf.Floor(totalXP / 4f);

        // Divide by 10 and floor to get the requirement amount
        int requirement = Mathf.FloorToInt(totalXP / 10f);

        // Ensure minimum
        return Mathf.Max(minimumCost, requirement);
    }

    /// <summary>
    /// Get the cost for a specific index/level, using smooth interpolation between overrides
    /// </summary>
    public int GetCost(int index)
    {
        // If no overrides, use formula with scale
        if (costOverrides == null || costOverrides.Count == 0)
        {
            var baseCost = CalculateRequiredAmount(index);
            return Mathf.Max(minimumCost, Mathf.CeilToInt(baseCost * costScale));
        }

        // Sort overrides by index for interpolation
        var sortedOverrides = costOverrides.OrderBy(o => o.index).ToList();

        // Check if this index has an explicit override with cost >= 0
        var exactOverride = sortedOverrides.FirstOrDefault(o => o.index == index);
        if (exactOverride != null && exactOverride.cost >= 0)
        {
            return Mathf.Max(minimumCost, exactOverride.cost);
        }

        // Find the two closest override points to interpolate between
        CostOverride lowerBound = null;
        CostOverride upperBound = null;

        foreach (var o in sortedOverrides)
        {
            if (o.index < index)
            {
                lowerBound = o;
            }
            else if (o.index > index && upperBound == null)
            {
                upperBound = o;
                break;
            }
        }

        // Case 1: Before the first override - scale based on first override
        if (lowerBound == null && upperBound != null)
        {
            var baseCost = CalculateRequiredAmount(index);
            var upperBaseCost = CalculateRequiredAmount(upperBound.index);

            // Calculate scale factor from the upper bound
            var scaleFactor = (float)upperBound.cost / upperBaseCost;
            return Mathf.Max(minimumCost, Mathf.CeilToInt(baseCost * scaleFactor));
        }

        // Case 2: After the last override - scale based on last override
        if (lowerBound != null && upperBound == null)
        {
            var baseCost = CalculateRequiredAmount(index);
            var lowerBaseCost = CalculateRequiredAmount(lowerBound.index);

            // Calculate scale factor from the lower bound
            var scaleFactor = (float)lowerBound.cost / lowerBaseCost;
            return Mathf.Max(minimumCost, Mathf.CeilToInt(baseCost * scaleFactor));
        }

        // Case 3: Between two overrides - interpolate smoothly
        if (lowerBound != null && upperBound != null)
        {
            var currentBaseCost = CalculateRequiredAmount(index);
            var lowerBaseCost = CalculateRequiredAmount(lowerBound.index);
            var upperBaseCost = CalculateRequiredAmount(upperBound.index);

            // Calculate how far we are between the two bounds (in base cost space)
            var progressInBaseCost = (float)(currentBaseCost - lowerBaseCost) / (upperBaseCost - lowerBaseCost);
            progressInBaseCost = Mathf.Clamp01(progressInBaseCost);

            // Interpolate between the override values
            var interpolatedCost = Mathf.Lerp(lowerBound.cost, upperBound.cost, progressInBaseCost);
            return Mathf.Max(minimumCost, Mathf.CeilToInt(interpolatedCost));
        }

        // Fallback to formula with global scale
        var fallbackBaseCost = CalculateRequiredAmount(index);
        return Mathf.Max(minimumCost, Mathf.CeilToInt(fallbackBaseCost * costScale));
    }
}
