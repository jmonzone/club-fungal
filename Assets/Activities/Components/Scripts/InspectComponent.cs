using UnityEngine;

[CreateAssetMenu(fileName = "InspectComponent", menuName = "Club Fungal/Activities/Components/Inspect")]
public class InspectComponent : ActivityComponent
{
    [SerializeField] private float inspectDuration = 10f;
    [SerializeField] private bool autoCompleteOnInitialize = false;

    [SerializeField] private float remainingDuration;
    [SerializeField] private bool isComplete;
    private float lastUpdateTime;
    private Door assignedDoor;

    public float InspectDuration => inspectDuration;
    public float RemainingDuration => remainingDuration;
    public bool IsComplete => isComplete;
    public Door AssignedDoor => assignedDoor;

    public void SetDoor(Door door)
    {
        assignedDoor = door;
    }

    protected override void OnInitialize()
    {
        if (autoCompleteOnInitialize)
        {
            isComplete = true;
            remainingDuration = 0;
            Debug.Log($"InspectComponent Auto-Completed on Initialize");
            CompleteTask(networkRun, activityInstance);
        }
        else
        {
            isComplete = false;
            remainingDuration = inspectDuration;
            lastUpdateTime = Time.realtimeSinceStartup;
            Debug.Log($"InspectComponent Initialized - Duration: {inspectDuration}s");
        }
    }

    public override void DoUpdate(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        int unitCount = activityInstance.Units?.Count ?? 0;

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

                // Check if inspection is complete
                if (remainingDuration <= 0)
                {
                    isComplete = true;
                }
                remainingDuration = Mathf.Max(0, remainingDuration);
            }
        }
        else
        {
            // Update time even when not decreasing to avoid huge jump when unit is added
            lastUpdateTime = Time.realtimeSinceStartup;
        }
    }

    public void CompleteTask(NetworkRun networkRun, ActivityInstance activityInstance)
    {
        if (assignedDoor != null && networkRun != null && activityInstance != null)
        {
        }
    }
}
