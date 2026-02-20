using UnityEngine;

/// <summary>
/// Modifies behavior priority based on camera settle state.
/// Can block behaviors (use int.MinValue) or boost priority when camera is not settled.
/// Useful for behaviors that should wait or take precedence during camera transitions.
/// </summary>
[CreateAssetMenu(fileName = "CameraSettlePriorityModifier", menuName = "Club Fungal/Units/Priority Modifiers/Camera Settle Priority Modifier")]
public class CameraSettlePriorityModifier : BehaviourPriorityModifier
{
    [SerializeField] private CameraService cameraService;
    
    [Header("Priority Settings")]
    [Tooltip("Priority modifier when camera is NOT settled. Use int.MinValue to block, positive values to boost priority.")]
    [SerializeField] private int priorityWhenUnsettled = 75;
    
    [Tooltip("Priority modifier when camera IS settled.")]
    [SerializeField] private int priorityWhenSettled = 0;

    public override int GetPriorityModifier()
    {
        if (!cameraService) return priorityWhenSettled;
        return cameraService.HasSettled ? priorityWhenSettled : priorityWhenUnsettled;
    }
}
