# Resource Harvester Component - Usage Guide

## Overview

The Resource Harvester Component automatically collects resources when a unit stands on specific NavMesh terrain areas. Perfect for gathering resources from slow terrain, special zones, or custom areas.

## Setup

### 1. Create a Resource Harvester Definition Asset

Right-click in Project window → Create → Club Fungal → Units → Components → Resource Harvester Component

### 2. Configure the Definition

**NavMesh Settings:**

- **Harvest Area Index:** The NavMesh area to detect (e.g., 0 = walkable, 6 = slow terrain)
- **Nav Mesh Area Config:** (Optional) Reference to NavMeshAreaConfig for area checking

**Resource Settings:**

- **Resource Item:** Drag the Item asset you want to collect
- **Amount Per Harvest:** How many items to collect each time (default: 1)
- **Harvest Interval:** Time in seconds between harvests (default: 2.0)
- **Minimum Stay Duration:** How long unit must stay on terrain before harvesting starts (default: 0.5)

**Collection Target:**

- **Unit Inventory:** Stores in unit's InventoryComponent
- **Global Inventory:** Stores in global InventoryReference
- **Both:** Stores in both locations

### 3. Attach to Unit

1. Select your UnitController prefab
2. Find the "Component System" section in inspector
3. Add your Resource Harvester Definition to the list
4. (Optional) Also add an InventoryComponent if using UnitInventory target

### 4. (Optional) Add Debug Visualization

Add the `ResourceHarvesterDebugger` MonoBehaviour to your UnitController to see:

- Green sphere when unit is on harvest terrain
- Red sphere when unit is NOT on harvest terrain
- Console logs when harvesting occurs
- Scene view label showing harvest count

## Example Configurations

### Mushroom Gathering from Slow Terrain

```
Harvest Area Index: 6 (slow terrain)
Resource Item: Mushroom
Amount Per Harvest: 1
Harvest Interval: 3.0 seconds
Minimum Stay Duration: 1.0 second
Target: Unit Inventory
```

Use case: Units collect mushrooms while walking on slow terrain areas.

### Spore Collection from Special Zones

```
Harvest Area Index: 2 (custom jump area)
Resource Item: Spore
Amount Per Harvest: 5
Harvest Interval: 1.0 second
Minimum Stay Duration: 0.5 seconds
Target: Global Inventory
```

Use case: Units standing in special zones rapidly collect spores to global inventory.

### Resource Mining

```
Harvest Area Index: 6
Resource Item: Crystal
Amount Per Harvest: 1
Harvest Interval: 5.0 seconds
Minimum Stay Duration: 2.0 seconds
Target: Both
```

Use case: Units must stay still for 2 seconds before mining begins, then collect 1 crystal every 5 seconds.

## Runtime API

```csharp
// Get the harvester component
var harvester = unitController.GetComponentInstance<ResourceHarvesterComponentInstance>();

// Check if unit is on harvest terrain
bool isHarvesting = harvester.IsOnHarvestTerrain;

// Get total resources harvested
int total = harvester.TotalHarvested;

// Reset the harvest counter
harvester.ResetHarvestCount();

// Listen to events
harvester.OnResourceHarvested += (item, amount) => {
    Debug.Log($"Collected {amount}x {item.Name}!");
};

harvester.OnEnteredHarvestTerrain += () => {
    Debug.Log("Started harvesting");
};

harvester.OnExitedHarvestTerrain += () => {
    Debug.Log("Stopped harvesting");
};
```

## NavMesh Area Indices

Common NavMesh areas in Club Fungal:

- **0** - Walkable (default)
- **1** - Not Walkable
- **2** - Jump
- **6** - Slow Terrain

You can find or configure these in Window → AI → Navigation → Areas

## Troubleshooting

**Unit not harvesting:**

- Check unit has NavMeshAgent and is on NavMesh
- Verify Harvest Area Index matches the terrain
- Ensure Resource Item is assigned
- Check minimum stay duration isn't too long
- Add ResourceHarvesterDebugger to visualize state

**Resources not appearing:**

- For Unit Inventory: Ensure unit has InventoryComponent attached
- For Global Inventory: Verify Global Inventory reference is set
- Check console for "inventory full" messages

**Harvesting too fast/slow:**

- Adjust Harvest Interval
- Adjust Minimum Stay Duration
- Check that unit is staying still on terrain
