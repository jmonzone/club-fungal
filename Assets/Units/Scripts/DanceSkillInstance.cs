using System.Collections.Generic;
using UnityEngine;

public class DanceSkillInstance : SkillInstance
{
    [SerializeField] private List<DanceMoveInstance> moves;

    public List<DanceMoveInstance> Moves => moves;

    public DanceSkillInstance(UnitInstance unit, Skill skill, float xp) : base(unit, skill, xp) { }

    protected override void InitializeSkillSpecifics()
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
                RegisterMove(move);
            }
        }
    }

    protected override void OnLevelUpSkillSpecifics()
    {
        foreach (var move in unit.Species.Moves)
        {
            if (move.LevelRequirement == level)
            {
                RegisterMove(move);
                Milestones.Add(move);
            }
        }
    }

    private void RegisterMove(DanceMove move)
    {
        var moveInstance = ScriptableObject.CreateInstance<DanceMoveInstance>();
        moveInstance.Initialize(move, this);
        moves.Add(moveInstance);
    }
}
