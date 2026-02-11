using UnityEngine;

public abstract class Upgrade : ScriptableObject
{
    [SerializeField] private string upgradeName;
    [SerializeField] private string description;
    
    public string UpgradeName => upgradeName;
    public string Description => description;
    
    public abstract void Apply(NetworkRun networkRun, ActivityInstance activityInstance);
}
