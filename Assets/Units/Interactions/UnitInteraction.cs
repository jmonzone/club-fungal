using UnityEngine;

public abstract class UnitInteraction : ScriptableObject
{
    [SerializeField] private string id;
    public string ID => id;

    public virtual void StartInteraction(UnitController source, UnitController target)
    {
    }
}
