using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [SerializeField] private DoorController otherDoor;
    [SerializeField] private PlayerReference playerReference;

    Transform ITarget.Transform => transform;

    void IInteractable.Select()
    {
        Debug.Log($"Door selected name: {gameObject.name} going to door: {otherDoor.gameObject.name} at position: {otherDoor.transform.position}");
        playerReference.TeleportPlayer(otherDoor.transform.position + otherDoor.transform.forward * -2f);
    }
}
