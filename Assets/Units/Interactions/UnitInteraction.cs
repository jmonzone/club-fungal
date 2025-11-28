using UnityEngine;

public abstract class UnitInteraction : ScriptableObject
{
    public virtual void StartInteraction(UnitController source, UnitController target)
    {
    }
}
