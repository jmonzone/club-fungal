using System;
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

    public void SetTemplate(ActivityReference template)
    {
        this.template = template;
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
