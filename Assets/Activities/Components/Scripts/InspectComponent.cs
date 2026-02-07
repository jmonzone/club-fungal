using UnityEngine;

[CreateAssetMenu(fileName = "InspectComponent", menuName = "Club Fungal/Activities/Components/Inspect")]
public class InspectComponent : ActivityComponent
{
    [SerializeField] private float inspectDuration = 10f;

    [SerializeField] private float remainingDuration;
    private float lastUpdateTime;
    [SerializeField] private bool isInitialized;

    public float InspectDuration => inspectDuration;
    public float RemainingDuration => remainingDuration;

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        if (!isInitialized)
        {
            remainingDuration = inspectDuration;
            lastUpdateTime = Time.realtimeSinceStartup;
            isInitialized = true;
        }

        int unitCount = activityInstance.Units?.Count ?? 0;

        Debug.Log($"InspectComponent Update - Remaining Duration: {remainingDuration:F2}s, Units: {unitCount}");
        // Only decrease if units are assigned
        if (unitCount > 0)
        {
            float currentTime = Time.realtimeSinceStartup;
            float deltaTime = currentTime - lastUpdateTime;
            lastUpdateTime = currentTime;

            if (remainingDuration > 0)
            {
                // Scale by number of units
                remainingDuration -= deltaTime * unitCount;
                remainingDuration = Mathf.Max(0, remainingDuration);
            }
        }
        else
        {
            // Update time even when not decreasing to avoid huge jump when unit is added
            lastUpdateTime = Time.realtimeSinceStartup;
        }
    }
}
