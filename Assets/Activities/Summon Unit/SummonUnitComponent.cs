using UnityEngine;

[CreateAssetMenu(fileName = "SummonUnit Component", menuName = "Club Fungal/Activities/Components/SummonUnit")]
public class SummonUnitComponent : ActivityComponent
{
    // Add your component fields here
    // Example:
    // [SerializeField] private float updateInterval = 1f;
    // [SerializeField] private int value = 1;

    protected override void OnInitialize()
    {
        // Initialize your component here
        Debug.Log($"SummonUnitComponent initialized");
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        // Update logic called during network run
        // Example: Process units, update resources, etc.
    }
}
