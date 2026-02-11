using System;
using System.Collections.Generic;
using UnityEngine;

// ActivityInstance represents a runtime activity with independent component state
[Serializable]
public class ActivityInstance
{
    [SerializeField] private ActivityReference template;
    [SerializeField] private ActivityData data;
    [SerializeField] private List<ActivityComponent> runtimeComponents;

    public ActivityData Data => data;
    public string Id => data.id;
    public string Name => data.name;
    public ActivityReference Template => template;
    public List<UnitInstance> Units => data.units;
    public List<ActivityComponent> RuntimeComponents => runtimeComponents;

    // Constructor - creates a new instance with copied components
    public ActivityInstance(NetworkRun networkRun, ActivityReference originalTemplate)
    {
        // Keep reference to original template
        this.template = originalTemplate;

        // Create a runtime copy of the activity reference with copied components
        var activityRefCopy = ScriptableObject.Instantiate(originalTemplate);
        activityRefCopy.name = originalTemplate.name;

        // Copy all components so each room has independent state
        if (activityRefCopy.Components != null && activityRefCopy.Components.Count > 0)
        {
            var copiedComponents = new List<ActivityComponent>();
            var originalToCopy = new Dictionary<ActivityComponent, ActivityComponent>();

            foreach (var component in activityRefCopy.Components)
            {
                if (component != null)
                {
                    var componentCopy = ScriptableObject.Instantiate(component);
                    componentCopy.name = component.name;
                    copiedComponents.Add(componentCopy);
                    originalToCopy[component] = componentCopy;
                }
            }

            // Allow components to remap their internal references
            foreach (var component in copiedComponents)
            {
                component.RemapReferences(originalToCopy);
            }

            // Replace the components list with the copied ones
            var componentsField = typeof(ActivityReference).GetField("components",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            componentsField?.SetValue(activityRefCopy, copiedComponents);

            // Store runtime components
            runtimeComponents = copiedComponents;
        }

        // Create activity data
        data = new ActivityData
        {
            id = UnitInstance.GenerateMongoLikeId(),
            name = originalTemplate.name
        };

        // Initialize runtime components
        if (runtimeComponents != null && runtimeComponents.Count > 0)
        {
            foreach (var component in runtimeComponents)
            {
                if (component != null)
                {
                    component.Initialize(networkRun, this);
                }
            }
        }
    }

    public void AddUnit(UnitInstance unit, List<ActivityInstance> allActivities = null)
    {
        // Remove unit from any other activity first (units can only be in one activity at a time)
        if (allActivities != null)
        {
            foreach (var otherActivity in allActivities)
            {
                if (otherActivity != null && otherActivity != this && otherActivity.Units != null)
                {
                    if (otherActivity.Units.Contains(unit))
                    {
                        otherActivity.RemoveUnit(unit);
                        Debug.Log($"Removed {unit.DisplayName} from {otherActivity.Name}");
                    }
                }
            }
        }

        // Add unit to this activity
        if (data.units == null)
        {
            data.units = new List<UnitInstance>();
        }
        if (!data.units.Contains(unit))
        {
            data.units.Add(unit);
        }
    }

    public void RemoveUnit(UnitInstance unit)
    {
        if (data.units != null)
        {
            data.units.Remove(unit);
        }
    }

    public void Update()
    {
        // Activity update logic here
    }

    public void Update(NetworkRun networkRun)
    {
        if (runtimeComponents != null)
        {
            // Build set of controlled components that should not update
            var controlledComponents = new HashSet<ActivityComponent>();
            foreach (var component in runtimeComponents)
            {
                if (component != null)
                {
                    var controlled = component.GetControlledComponents();
                    if (controlled != null)
                    {
                        foreach (var controlledComponent in controlled)
                        {
                            controlledComponents.Add(controlledComponent);
                        }
                    }
                }
            }

            // Update components that should update and are not controlled
            foreach (var component in runtimeComponents)
            {
                if (component != null &&
                    component.ShouldUpdate() &&
                    !controlledComponents.Contains(component))
                {
                    component.DoUpdate(networkRun, this);
                }
            }
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is ActivityInstance other)
        {
            return Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(ActivityInstance left, ActivityInstance right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return true;
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
        return left.Id == right.Id;
    }

    public static bool operator !=(ActivityInstance left, ActivityInstance right)
    {
        return !(left == right);
    }
}
