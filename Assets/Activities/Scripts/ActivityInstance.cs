using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActivityInstance
{
    [SerializeField] private ActivityReference template;
    [SerializeField] private ActivityData data;

    public ActivityData Data => data;
    public string Id => data.id;
    public string Name => data.name;
    public ActivityReference Template => template;
    public List<UnitInstance> Units => data.units;

    public ActivityInstance(ActivityData data)
    {
        var initData = data;
        if (string.IsNullOrEmpty(initData.id)) initData.id = UnitInstance.GenerateMongoLikeId();
        this.data = initData;
    }

    public ActivityInstance(ActivityReference template)
    {
        this.template = template;
        data = new ActivityData
        {
            id = UnitInstance.GenerateMongoLikeId(),
            name = template.name
        };
    }

    // Copy constructor - creates a new instance with copied components but original template
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
            foreach (var component in activityRefCopy.Components)
            {
                if (component != null)
                {
                    var componentCopy = ScriptableObject.Instantiate(component);
                    componentCopy.name = component.name;
                    copiedComponents.Add(componentCopy);
                }
            }

            // Replace the components list with the copied ones
            var componentsField = typeof(ActivityReference).GetField("components",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            componentsField?.SetValue(activityRefCopy, copiedComponents);
        }

        // Create activity data
        data = new ActivityData
        {
            id = UnitInstance.GenerateMongoLikeId(),
            name = originalTemplate.name
        };

        // Initialize all components
        if (activityRefCopy.Components != null)
        {
            var componentsCopy = new List<ActivityComponent>(activityRefCopy.Components);
            foreach (var component in componentsCopy)
            {
                if (component != null)
                {
                    component.Initialize(networkRun, this);
                }
            }
        }
    }

    public void SetTemplate(ActivityReference template)
    {
        this.template = template;
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
        if (template?.Components != null)
        {
            foreach (var component in template.Components)
            {
                if (component != null)
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
