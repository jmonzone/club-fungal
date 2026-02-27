using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Defines a unit type with configurable properties like inventory capacity.
/// </summary>
[CreateAssetMenu(fileName = "New Unit Type", menuName = "Club Fungal/Units/Unit Type")]
public class UnitType : GURUObject
{
    [Serializable]
    public class ResourceSpeedBonus
    {
        public ItemTemplate resource; // The resource item
        public float speedBonus = 2f; // Speed multiplier when collecting this resource
    }

    [Serializable]
    public class TerrainSpeedModifier
    {
        [NavMeshArea]
        [Tooltip("NavMesh area (e.g., Walkable, Slow Terrain)")]
        public int areaIndex;
        [Tooltip("Speed multiplier on this terrain (e.g., 1.5 = 50% faster, 0.5 = half speed)")]
        public float speedMultiplier = 1f;
    }

    [SerializeField] private int inventoryBonus = 0; // Bonus inventory slots added to base capacity
    [SerializeField] private List<ResourceSpeedBonus> resourceSpeedBonuses = new List<ResourceSpeedBonus>();
    [SerializeField] private List<TerrainSpeedModifier> terrainSpeedModifiers = new List<TerrainSpeedModifier>();
    [SerializeField] private AbilityDefinition abilityDefinition; // Type-specific ability (e.g., Fly for birds, Dive for fish)
    [SerializeField][NavMeshAreaMask] private int preferredTerrainMaskOverride = -1; // Manual override for preferred terrain mask (-1 = auto-calculate from speed modifiers)

    public int InventoryBonus => inventoryBonus;
    public List<ResourceSpeedBonus> ResourceSpeedBonuses => resourceSpeedBonuses;
    public List<TerrainSpeedModifier> TerrainSpeedModifiers => terrainSpeedModifiers;
    public AbilityDefinition AbilityDefinition => abilityDefinition;

    public float GetSpeedBonusForResource(ItemTemplate item)
    {
        if (item == null || resourceSpeedBonuses == null)
            return 1f;

        foreach (var bonus in resourceSpeedBonuses)
        {
            if (bonus.resource == item)
            {
                return bonus.speedBonus;
            }
        }

        return 1f;
    }

    public float GetSpeedMultiplierForTerrain(int areaIndex)
    {
        if (terrainSpeedModifiers == null)
            return 1f;

        foreach (var modifier in terrainSpeedModifiers)
        {
            if (modifier.areaIndex == areaIndex)
            {
                return modifier.speedMultiplier;
            }
        }

        return 1f;
    }

    /// <summary>
    /// Get the NavMesh area mask for preferred terrains (areas with speed >= 1.0).
    /// Returns NavMesh.AllAreas if no terrain modifiers configured.
    /// Can be manually overridden via preferredTerrainMaskOverride field.
    /// </summary>
    public int GetPreferredTerrainMask()
    {
        // Use manual override if set
        if (preferredTerrainMaskOverride != -1)
            return preferredTerrainMaskOverride;

        // Auto-calculate from terrain speed modifiers
        if (terrainSpeedModifiers == null || terrainSpeedModifiers.Count == 0)
            return NavMesh.AllAreas;

        int mask = 0;
        foreach (var modifier in terrainSpeedModifiers)
        {
            if (modifier.speedMultiplier >= 1f)
            {
                mask |= (1 << modifier.areaIndex);
            }
        }

        return mask == 0 ? NavMesh.AllAreas : mask;
    }
}
