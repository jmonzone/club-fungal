using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnitData
{
    public string Id;
    public string DisplayName;
    public UnitSpecies Species;
    public float FriendshipXP;
    public int FriendshipLevel;
    public Element Element;
    public Job Job;
    public ColorPalette ColorPalette;
    public string Scene;

    // For JSON serialization
    public string name;
    public string displayName;
    public float friendshipXP;
    public int friendshipLevel;
    public string element;
    public string job;
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

    public Dictionary<Skill, SkillInstance> Skills = new Dictionary<Skill, SkillInstance>();

    [SerializeField] private List<UnitInstance> friends;
    [SerializeField] private List<UnitMoment> moments;

    public UnitData Data => data;
    public string Id => data.Id;
    public string DisplayName => data.DisplayName;
    public UnitSpecies Species => data.Species;
    public Element Element => data.Element;
    public Job Job => data.Job;
    public ColorPalette ColorPalette => data.ColorPalette;

    public int FriendshipLevel => SkillInstance.GetLevelFromXP(data.FriendshipXP);
    public float FriendshipXP => data.FriendshipXP;
    public bool IsFriends => FriendshipLevel > 1;

    public List<UnitInstance> Friends => friends;
    public List<UnitMoment> Moments => moments;

    public UnitTemplate Template => template;

    public event UnityAction<float> OnXpChanged;

    public UnitInstance(UnitData data)
    {
        var initData = data;
        if (string.IsNullOrEmpty(initData.Id)) initData.Id = GenerateMongoLikeId();
        this.data = initData;
        friends = new List<UnitInstance>();
        moments = new List<UnitMoment>();
    }

    public void SetTemplate(UnitTemplate template)
    {
        this.template = template;
    }

    public void InitializeSkills(List<SkillInstance> skills)
    {
        this.skills = skills;
        Skills = new Dictionary<Skill, SkillInstance>();
        foreach (var skill in this.skills)
        {
            Skills.Add(skill.Skill, skill);
            skill.OnXpChanged += value => OnXpChanged?.Invoke(value);
        }
    }

    public void InitializeMoments(List<UnitMoment> moments)
    {
        this.moments = moments;
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

}
