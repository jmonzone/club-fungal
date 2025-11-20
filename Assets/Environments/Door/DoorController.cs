using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    Transform ITarget.Transform => transform;

    void IInteractable.Select()
    {
        Debug.Log($"Door selected name: {gameObject.name}");
    }
}
