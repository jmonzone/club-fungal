using UnityEngine;

[CreateAssetMenu(fileName = "SkillSpeedUpgrade", menuName = "Club Fungal/Upgrades/Skill Speed Upgrade")]
public class SkillSpeedUpgrade : Upgrade
{
    [SerializeField] private Skill skill;
    [SerializeField] private float speedBonusMultiplier = 0.1f; // 10% speed increase by default
    
    public Skill Skill => skill;
    public float SpeedBonusMultiplier => speedBonusMultiplier;
    
    public override void Apply(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Apply the speed bonus to the skill
        Debug.Log($"Applying {UpgradeName}: Increasing {skill?.name} speed by {speedBonusMultiplier * 100}%");
    }
}
