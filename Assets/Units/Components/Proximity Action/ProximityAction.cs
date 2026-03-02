using UnityEngine;

/// <summary>
/// Wrapper for proximity actions that delegates to a UnitAction strategy.
/// Allows flexible action composition without inheritance.
/// </summary>
[CreateAssetMenu(fileName = "ProximityAction", menuName = "Club Fungal/Proximity Actions/Proximity Action")]
public class ProximityAction : ScriptableObject
{
    [SerializeField] private UnitAction action;

    public UnitAction Action => action;

    /// <summary>
    /// Called when the proximity action is triggered.
    /// Delegates to the configured UnitAction.
    /// </summary>
    /// <param name="controller">The unit controller this action is attached to</param>
    public void Execute(UnitController controller)
    {
        if (action != null)
        {
            action.Execute(controller);
        }
        else
        {
            Debug.LogWarning("ProximityAction: No action assigned");
        }
    }
}
