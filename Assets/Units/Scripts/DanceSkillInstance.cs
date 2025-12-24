using System.Collections.Generic;
using UnityEngine;

public class DanceSkillInstance : SkillInstance
{
    [SerializeField] private List<DanceMoveInstance> moves;

    public List<DanceMoveInstance> Moves => moves;

    public DanceSkillInstance(UnitInstance unit, Skill skill, float xp) : base(unit, skill, xp) { }

    protected override void InitializeSkillSpecifics(UnitInstance unit)
    {
        moves = new List<DanceMoveInstance>();

        if (!unit.Species)
        {
            Debug.LogWarning($"UnitSkill: missing unit data {unit.Id}");
            return;
        }

        // Debug.Log($"Initializing skill {skill.Id} for unit {unit.Data.Name} at level {level} with {xp} XP");
        foreach (var move in unit.Species.Moves)
        {
            if (level >= move.LevelRequirement)
            {
                RegisterMove(move, unit);
            }
        }
    }

    protected override void OnLevelUpSkillSpecifics(UnitInstance unit)
    {
        foreach (var move in unit.Species.Moves)
        {
            if (move.LevelRequirement == level)
            {
                RegisterMove(move, unit);
                Milestones.Add(move);
            }
        }
    }

    private void RegisterMove(DanceMove move, UnitInstance unit)
    {
        var moveInstance = ScriptableObject.CreateInstance<DanceMoveInstance>();
        moveInstance.Initialize(move, this, unit);
        moves.Add(moveInstance);
    }
}
