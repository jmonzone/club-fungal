using System;
using System.Collections.Generic;
using System.Linq;
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
    public List<AbilityDefinition> abilities;

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
    [SerializeReference] private List<AbilityInstance> abilities;

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
    [SerializeField] private float energy = 100f; // Energy level (0-100)
    [SerializeField] private bool isExhausted = false; // Debuff when energy is depleted

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
    public List<AbilityInstance> Abilities => abilities;

    public UnitTemplate Template => template;
    public Inventory Inventory => inventory;
    public float Energy
    {
        get => energy;
        set => energy = Mathf.Clamp(value, 0f, 100f);
    }
    public bool IsExhausted
    {
        get => isExhausted;
        set => isExhausted = value;
    }

    public event UnityAction<float> OnXpChanged;

    public UnitInstance(UnitData data)
    {
        var initData = data;
        if (string.IsNullOrEmpty(initData.id)) initData.id = GenerateMongoLikeId();
        this.data = initData;
        friends = new List<string>();
        moments = new List<UnitMoment>();
        abilities = new List<AbilityInstance>();
        inventory = new Inventory(GetInventoryCapacityForSpecies(data.species));
    }

    private static int GetInventoryCapacityForSpecies(UnitSpecies species)
    {
        const int baseCapacity = 10;
        if (species?.Type == null) return baseCapacity;
        return baseCapacity + species.Type.InventoryBonus;
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

    public void InitializeAbilities(List<AbilityInstance> abilities)
    {
        this.abilities = abilities;
        data.abilities = abilities.Select(a => a.Definition).ToList();
    }

    /// <summary>
    /// Update all abilities (cooldowns, timers, etc.)
    /// Should be called every frame by the controller.
    /// </summary>
    public void UpdateAbilities(float deltaTime)
    {
        if (abilities == null) return;

        foreach (var ability in abilities)
        {
            ability?.Update(deltaTime);
        }
    }

    /// <summary>
    /// Get ability instance by definition type.
    /// </summary>
    public T GetAbility<T>() where T : AbilityInstance
    {
        if (abilities == null) return null;

        foreach (var ability in abilities)
        {
            if (ability is T match)
            {
                return match;
            }
        }
        return null;
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

