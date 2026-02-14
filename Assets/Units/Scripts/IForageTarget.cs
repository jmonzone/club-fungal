using UnityEngine;

/// <summary>
/// Interface for objects that can be foraged (inspected and collected) by units
/// </summary>
public interface IForageTarget : ITarget
{
    void OnForaged(UnitController forager);
    bool IsAvailable { get; }
}
