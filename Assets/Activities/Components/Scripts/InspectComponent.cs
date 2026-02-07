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

    public override void Initialize(NetworkRun networkRun, ActivityInstance activityInstance)
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
            // Create a new ResourceCondition for the door using NetworkRun's method
            // This will select a random item from available activities and scale the count using RuneScape formula
            var resourceCondition = networkRun.CreateResourceConditionForDoor();

            if (resourceCondition != null)
            {
                // Keep door locked - it will be unlocked when resources are collected

                // Add the resource condition to the door
                if (assignedDoor.conditions == null)
                {
                    assignedDoor.conditions = new System.Collections.Generic.List<DoorCondition>();
                }
                assignedDoor.conditions.Add(resourceCondition);

                Debug.Log($"Inspection completed! Resource condition assigned: {resourceCondition.GetDescription()}. Door remains locked until resources collected.");

                // Create and add UnlockComponent to replace InspectComponent
                var unlockComponent = ScriptableObject.CreateInstance<UnlockComponent>();
                unlockComponent.name = "UnlockComponent";
                unlockComponent.SetDoorAndCondition(assignedDoor, resourceCondition);

                // Replace InspectComponent with UnlockComponent in the activity's template
                if (activityInstance.Template?.Components != null)
                {
                    var componentsField = typeof(ActivityReference).GetField("components",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var componentsList = componentsField?.GetValue(activityInstance.Template) as System.Collections.Generic.List<ActivityComponent>;

                    if (componentsList != null)
                    {
                        // Find and replace this InspectComponent with UnlockComponent
                        int index = componentsList.IndexOf(this);
                        if (index >= 0)
                        {
                            componentsList[index] = unlockComponent;
                            unlockComponent.Initialize(networkRun, activityInstance);
                            Debug.Log($"Replaced InspectComponent with UnlockComponent");
                        }
                    }
                }
            }
        }
    }
}
