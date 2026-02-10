using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeActivityComponent", menuName = "Club Fungal/Activities/Components/Upgrade Activity")]
public class UpgradeActivityComponent : ActivityComponent
{
    // Add your component fields here
    // Example:
    // [SerializeField] private float updateInterval = 1f;
    // [SerializeField] private int value = 1;

    public override void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Initialize your component here
        Debug.Log($"UpgradeActivityComponent initialized");
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Update logic called during network run
        // Example: Process units, update resources, etc.
    }
}
