using UnityEngine;

/// <summary>
/// Interface for unit behaviors that can receive return position commands.
/// Used by NetworkRunPartyTether to coordinate unit movement with camera.
/// </summary>
public interface IReturnPositionable
{
    void SetReturnPosition(Vector3 position, bool shouldTeleport = false);
}
