using System;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.Events;

public interface IMilestone
{
    public string Label { get; }
    public Sprite Sprite { get; }
    public string Description { get; }
}

[Serializable]
public class SkillInstance
{
    [SerializeField] protected Skill skill;
    [SerializeField] protected int level;
    [SerializeField] protected float xp;

    public string Label => skill.Id;
    public Skill Skill => skill;
    public int Level => level;
    public float XP => xp;
    public List<IMilestone> Milestones { get; private set; }

    public event UnityAction<float> OnXpChanged;
    public event UnityAction OnLevelUp;

    public SkillInstance(UnitInstance unit, Skill skill, float xp)
    {
        this.skill = skill;
        this.xp = xp;
        level = GetLevelFromXP(xp);

        InitializeSkillSpecifics(unit);
    }

    protected virtual void InitializeSkillSpecifics(UnitInstance unit) { }

    protected virtual void OnLevelUpSkillSpecifics(UnitInstance unit) { }
    public void IncreaseSkillXP(UnitInstance unit, float value)
    {
        //Debug.Log("increasing skill xp");

        var previousLevel = level;

        xp += value;
        OnXpChanged?.Invoke(value);

        level = GetLevelFromXP(xp);

        if (previousLevel != level)
        {
            Milestones = new List<IMilestone>();
            OnLevelUpSkillSpecifics(unit);
            OnLevelUp?.Invoke();
        }
    }

    public static int GetLevelFromXP(float xp)
    {
        int level = 1;
        double points = 0;

        for (int lvl = 1; lvl <= 120; lvl++) // RuneScape goes to 99/120, you can adjust cap
        {
            points += Math.Floor(lvl + 300 * Math.Pow(2, lvl / 7.0));
            double output = Math.Floor(points / (4));

            if (output > xp)
            {
                level = lvl;
                break;
            }
        }

        return level;
    }


    public static int GetXPFromLevel(int level)
    {
        double points = 0;

        for (int lvl = 1; lvl < level; lvl++)
        {
            points += Math.Floor(lvl + 300 * Math.Pow(2, lvl / 7.0));
        }

        return (int)Math.Floor(points / (4));
    }
}
