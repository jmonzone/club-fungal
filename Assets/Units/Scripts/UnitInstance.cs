using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnitData
{
    public string id;
    public string name;
    public string displayName;
    public UnitSpecies species;
    [JsonConverter(typeof(GURUConverter))] public Job job;
    public Element element;
    [JsonConverter(typeof(GURUConverter))] public ColorPalette colorPalette;
    public string scene;
    public int friendshipLevel;
    public float friendshipXP;
    public List<string> friends;
    public List<InteractionData> interactions;
    public List<SkillData> skills;

    [Serializable]
    public class SkillData
    {
        public string id;
        public int level;
        public float xp;
    }

    [Serializable]
    public class InteractionData
    {
        public string id;
        public bool isComplete;
    }
}

[Serializable]
public class UnitInstance
{
    [SerializeField] private UnitTemplate template;
    [SerializeField] private UnitData data;

    [SerializeField] private List<SkillInstance> skills;

    // Build dictionary on-demand from serialized list
    private Dictionary<Skill, SkillInstance> _skillsCache;
    public Dictionary<Skill, SkillInstance> Skills
    {
        get
        {
            if (_skillsCache == null && skills != null)
            {
                _skillsCache = new Dictionary<Skill, SkillInstance>();
                foreach (var skill in skills)
                {
                    if (skill?.Skill != null)
                    {
                        _skillsCache[skill.Skill] = skill;
                    }
                }
            }
            return _skillsCache;
        }
    }

    [SerializeField] private List<string> friends;
    [SerializeField] private List<UnitMoment> moments;
    [SerializeField] private Inventory inventory = new Inventory();

    public UnitData Data => data;
    public string Id => data.id;
    public string DisplayName => data.displayName;
    public UnitSpecies Species => data.species;
    public Element Element => data.element;
    public Job Job => data.job;
    public ColorPalette ColorPalette => data.colorPalette;

    public int FriendshipLevel => SkillInstance.GetLevelFromXP(data.friendshipXP);
    public float FriendshipXP => data.friendshipXP;
    public bool IsFriends => FriendshipLevel > 1;

    public List<string> Friends => friends;
    public List<UnitMoment> Moments => moments;

    public UnitTemplate Template => template;
    public Inventory Inventory => inventory;

    public event UnityAction<float> OnXpChanged;

    public UnitInstance(UnitData data)
    {
        var initData = data;
        if (string.IsNullOrEmpty(initData.id)) initData.id = GenerateMongoLikeId();
        this.data = initData;
        friends = new List<string>();
        moments = new List<UnitMoment>();
        inventory = new Inventory();
    }

    public void SetTemplate(UnitTemplate template)
    {
        this.template = template;
    }

    public void InitializeSkills(List<SkillInstance> skills)
    {
        this.skills = skills;
        _skillsCache = null; // Clear cache so it rebuilds on next access
        foreach (var skill in this.skills)
        {
            skill.OnXpChanged += value => OnXpChanged?.Invoke(value);
        }
    }

    public void InitializeMoments(List<UnitMoment> moments)
    {
        this.moments = moments;
    }

    public void ClaimItemsToInventory(ItemTemplate item, Inventory targetInventory)
    {
        var count = inventory.GetItemCount(item);
        if (count > 0)
        {
            inventory.RemoveItem(item, count);
            for (int i = 0; i < count; i++)
            {
                targetInventory.AddItem(item);
            }
        }
    }

    public static string GenerateMongoLikeId()
    {
        byte[] bytes = new byte[12];

        // 4 bytes: current Unix timestamp
        uint timestamp = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        BitConverter.GetBytes(timestamp).CopyTo(bytes, 0);

        // 3 bytes: random machine identifier
        var machine = new byte[3];
        new System.Random().NextBytes(machine);
        Array.Copy(machine, 0, bytes, 4, 3);

        // 2 bytes: process id (or random)
        ushort pid = (ushort)UnityEngine.Random.Range(0, ushort.MaxValue);
        BitConverter.GetBytes(pid).CopyTo(bytes, 7);

        // 3 bytes: incrementing counter (random for simplicity)
        var counter = new byte[3];
        new System.Random().NextBytes(counter);
        Array.Copy(counter, 0, bytes, 9, 3);

        // Convert to 24-character hex string
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    public override bool Equals(object obj)
    {
        if (obj is UnitInstance other)
        {
            return Id == other.Id;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(UnitInstance left, UnitInstance right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return true;
        if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
        return left.Id == right.Id;
    }

    public static bool operator !=(UnitInstance left, UnitInstance right)
    {
        return !(left == right);
    }
}

