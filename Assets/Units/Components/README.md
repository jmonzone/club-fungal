# Unit Component System

## Overview

The Unit Component System provides a ScriptableObject-based architecture for attaching modular, reusable components to units in Club Fungal. This follows the same pattern as other systems in the codebase (like Skills, Services, etc.).

## Architecture

### UnitComponentDefinition (ScriptableObject)

Base class for defining component types. These are assets you create in the project that define component behavior and settings.

**Location:** `Assets/Units/Scripts/UnitComponentDefinition.cs`

```csharp
public abstract class UnitComponentDefinition : GURUObject
{
    public abstract UnitComponentInstance CreateInstance(UnitController controller);
}
```

### UnitComponentInstance (Runtime)

Base class for runtime component instances. These wrap the ScriptableObject definitions and manage actual runtime state.

**Location:** `Assets/Units/Scripts/UnitComponentInstance.cs`

```csharp
public abstract class UnitComponentInstance
{
    public virtual void OnInitialize() { }
    public virtual void OnUpdate() { }
    public virtual void OnDestroy() { }
    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
}
```

### UnitController (Manager)

Manages a list of component instances and handles their lifecycle.

**Key Features:**

- Attach component definitions in the inspector
- Components initialize automatically when unit is initialized
- Components update every frame via `UpdateComponents()`
- Add/remove components at runtime
- Query components by type

## Lifecycle

1. **Initialization:** Component definitions are converted to instances in `UnitController.Initialize()`
2. **Update:** All components receive `OnUpdate()` calls every frame
3. **Destruction:** Components are cleaned up in `OnDestroy()`

## Usage

### Creating a Component Definition

1. Create a new ScriptableObject class inheriting from `UnitComponentDefinition`:

```csharp
[CreateAssetMenu(fileName = "MyComponent", menuName = "Club Fungal/Units/Components/My Component")]
public class MyComponentDefinition : UnitComponentDefinition
{
    [SerializeField] private float someValue = 10f;
    public float SomeValue => someValue;

    public override UnitComponentInstance CreateInstance(UnitController controller)
    {
        return new MyComponentInstance(this, controller);
    }
}
```

2. Create the corresponding instance class in the same file:

```csharp
[System.Serializable]
public class MyComponentInstance : UnitComponentInstance
{
    public MyComponentDefinition MyDefinition => definition as MyComponentDefinition;

    public MyComponentInstance(MyComponentDefinition definition, UnitController controller)
        : base(definition, controller)
    {
    }

    public override void OnInitialize()
    {
        base.OnInitialize();
        // Setup logic here
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        // Per-frame logic here
    }
}
```

### Attaching Components in Inspector

1. Select a UnitController prefab or instance
2. In the inspector, find the "Component System" section
3. Add your component definitions to the `Component Definitions` list
4. Components will be automatically instantiated when the unit initializes

### Runtime Component Management

```csharp
// Add a component at runtime
unitController.AddComponent(myComponentDefinition);

// Remove a component
var component = unitController.GetComponentInstance<MyComponentInstance>();
unitController.RemoveComponent(component);

// Get a single component
var healthComp = unitController.GetComponentInstance<HealthComponentInstance>();
healthComp?.TakeDamage(10f);

// Get all components of a type
var allMovementComps = unitController.GetComponentInstances<MovementComponentInstance>();
```

## Example Components

### HealthComponent

Manages unit health, damage, healing, and death.

**Location:** `Assets/Units/Scripts/Components/HealthComponent.cs`

```csharp
var health = unitController.GetComponentInstance<HealthComponentInstance>();
health.TakeDamage(25f);
health.Heal(10f);
health.OnDeath += () => Debug.Log("Unit died!");
```

### MovementComponent

Controls unit movement speed and NavMeshAgent settings.

**Location:** `Assets/Units/Scripts/Components/MovementComponent.cs`

```csharp
var movement = unitController.GetComponentInstance<MovementComponentInstance>();
movement.SetSpeedMultiplier(2f); // Double speed
movement.ResetSpeed();
```

### InventoryComponent

Manages item storage for units.

**Location:** `Assets/Units/Scripts/Components/InventoryComponent.cs`

```csharp
var inventory = unitController.GetComponentInstance<InventoryComponentInstance>();
inventory.AddItem("mushroom", 5);
bool hasMushrooms = inventory.HasItem("mushroom", 3);
inventory.RemoveItem("mushroom", 2);
```

### ResourceHarvesterComponent

Automatically collects resources when unit is standing on specific NavMesh terrain areas.

**Location:** `Assets/Units/Scripts/Components/ResourceHarvesterComponent.cs`

```csharp
var harvester = unitController.GetComponentInstance<ResourceHarvesterComponentInstance>();
harvester.OnResourceHarvested += (item, amount) => Debug.Log($"Harvested {amount}x {item.Name}");
harvester.OnEnteredHarvestTerrain += () => Debug.Log("Entered harvest area!");
int total = harvester.TotalHarvested;
```

**Configuration:**

- **Harvest Area Index:** NavMesh area to harvest from (e.g., 0 = walkable, 6 = slow terrain)
- **Resource Item:** The item to collect
- **Harvest Interval:** Time between collections
- **Minimum Stay Duration:** How long unit must stay on terrain before harvesting begins
- **Harvest Target:** Where to store resources (Unit Inventory, Global Inventory, or Both)

## Best Practices

1. **Definition = Data, Instance = Logic**
    - ScriptableObject definitions store configuration and settings
    - Instance classes handle runtime state and behavior

2. **Events for Communication**
    - Use UnityEvents to notify other systems of changes
    - Example: `OnDeath`, `OnItemAdded`, etc.

3. **Component Queries**
    - Always check for null when getting components
    - Use `GetComponentInstance<T>()` for required components
    - Use `GetComponentInstances<T>()` for optional/multiple components

4. **Performance**
    - Only update components that need per-frame logic
    - Use events to react to changes instead of polling

## Migration from MonoBehaviour Components

The old `UnitComponent` MonoBehaviour base class still exists for backward compatibility. To migrate:

1. Create a ScriptableObject definition for your component
2. Create an instance class that wraps the definition
3. Move logic from MonoBehaviour to instance class
4. Replace GetComponent calls with GetComponentInstance calls
5. Attach the new definition to UnitController instead of adding MonoBehaviour

## API Reference

### UnitController

```csharp
// Component Management
void AddComponent(UnitComponentDefinition definition)
void RemoveComponent(UnitComponentInstance instance)
T GetComponentInstance<T>() where T : UnitComponentInstance
List<T> GetComponentInstances<T>() where T : UnitComponentInstance

// Properties
List<UnitComponentInstance> ComponentInstances { get; }
```

### UnitComponentInstance

```csharp
// Lifecycle
virtual void OnInitialize()
virtual void OnUpdate()
virtual void OnDestroy()
virtual void OnEnable()
virtual void OnDisable()

// Properties
UnitComponentDefinition Definition { get; }
UnitController Controller { get; }
UnitInstance UnitInstance { get; }
```

## Troubleshooting

**Components not updating:**

- Ensure `OnUpdate()` is overridden in your instance class
- Check that the component was added to the controller

**Null reference on GetComponentInstance:**

- Component definition may not be attached in inspector
- Component may not have initialized yet
- Always null-check the result

**Component not initializing:**

- Override `OnInitialize()` in your instance class
- Call `base.OnInitialize()` at the start
- Check console for initialization errors
