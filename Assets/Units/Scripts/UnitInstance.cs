using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct UnitInstanceData
{
    public Unit Data;
    public string Id;
    public string DisplayName;
    public float FriendshipXP;
    public Element Element;
    public Job Job;
    public ColorPalette ColorPalette;
    public JObject Json;
}

[CreateAssetMenu(fileName = "UnitInstance", menuName = "Club Fungal/Units/Unit Instance")]
public class UnitInstance : ScriptableObject
{
    [SerializeField] private UnitInstanceData instanceData;

    [SerializeField] private List<UnitSkill> skills;

    public Dictionary<Skill, UnitSkill> Skills = new Dictionary<Skill, UnitSkill>();

    [SerializeField] private List<UnitInstance> friends;
    [SerializeField] private UnitInteraction interaction;

    public UnitInstanceData InstanceData => instanceData;
    public string Id => instanceData.Id;
    public string DisplayName => instanceData.DisplayName;
    public Unit Data => instanceData.Data;
    public Element Element => instanceData.Element;
    public Job Job => instanceData.Job;
    public ColorPalette ColorPalette => instanceData.ColorPalette;

    public int FriendshipLevel => UnitSkill.GetLevelFromXP(instanceData.FriendshipXP);
    public float FriendshipXP => instanceData.FriendshipXP;
    public bool IsFriends => FriendshipLevel > 1;

    public List<UnitInstance> Friends => friends;
    public UnitInteraction Interaction => interaction;

    public JObject Json => instanceData.Json;

    public event UnityAction<float> OnXpChanged;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(instanceData.DisplayName) && instanceData.Data != null)
        {
            var temp = instanceData;
            temp.DisplayName = instanceData.Data.Name;
            instanceData = temp;
        }
    }

    public void Initialize(UnitInstanceData data)
    {
        var initData = data;
        if (string.IsNullOrEmpty(initData.Id)) initData.Id = GenerateMongoLikeId();
        instanceData = initData;
        friends = new List<UnitInstance>();
    }

    public void InitializeSkills(List<UnitSkill> skills)
    {
        this.skills = skills;
        Skills = new Dictionary<Skill, UnitSkill>();
        foreach (var skill in this.skills)
        {
            Skills.Add(skill.Skill, skill);
            skill.OnXpChanged += value => OnXpChanged?.Invoke(value);
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

}
