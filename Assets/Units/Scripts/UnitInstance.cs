using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct UnitData
{
    public string Id;
    public string DisplayName;
    public UnitSpecies Species;
    public float FriendshipXP;
    public Element Element;
    public Job Job;
    public ColorPalette ColorPalette;
    public JObject Json;
}

[CreateAssetMenu(fileName = "UnitInstance", menuName = "Club Fungal/Units/Unit Instance")]
public class UnitInstance : ScriptableObject
{
    [SerializeField] private UnitInstance originalInstance;
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

    public JObject Json => data.Json;

    public UnitInstance OriginalInstance => originalInstance;

    public event UnityAction<float> OnXpChanged;

    public void SetOriginalInstance(UnitInstance original)
    {
        originalInstance = original;
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(data.DisplayName) && data.Species != null)
        {
            var temp = data;
            temp.DisplayName = data.Species.Id;
            data = temp;
        }
    }

    public void Initialize(UnitData data)
    {
        var initData = data;
        if (string.IsNullOrEmpty(initData.Id)) initData.Id = GenerateMongoLikeId();
        this.data = initData;
        friends = new List<UnitInstance>();
        moments = new List<UnitMoment>();
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
