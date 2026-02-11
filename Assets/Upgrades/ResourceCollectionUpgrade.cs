using UnityEngine;

[CreateAssetMenu(fileName = "ResourceCollectionUpgrade", menuName = "Club Fungal/Upgrades/Resource Collection Upgrade")]
public class ResourceCollectionUpgrade : Upgrade
{
    [SerializeField] private ItemTemplate resource;
    [SerializeField] private float increaseAmount;
    
    public ItemTemplate Resource => resource;
    public float IncreaseAmount => increaseAmount;
    
    public override void Apply(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Apply the resource collection increase to the activity
        Debug.Log($"Applying {UpgradeName}: Increasing {resource.name} collection by {increaseAmount}");
    }
}
