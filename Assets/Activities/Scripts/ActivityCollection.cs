using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ActivityCollection", menuName = "Club Fungal/Activities/Activity Collection")]
public class ActivityCollection : ScriptableObject
{
    [SerializeField] private List<ActivityReference> activities;

    public ActivityReference GetRandomActivity()
    {
        if (activities.Count == 0)
        {
            Debug.LogWarning("ActivityCollection: No activities available to select.");
            return null;
        }

        int randomIndex = Random.Range(0, activities.Count);
        var randomActivity = activities[randomIndex];

        var copy = Instantiate(randomActivity);
        return copy;
    }
}
