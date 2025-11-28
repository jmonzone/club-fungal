using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GreetingInteraction", menuName = "Club Fungal/Interactions/Greeting Interaction")]
public class GreetingInteraction : UnitInteraction
{
    [SerializeField] private DialogueReference dialogueReference;
    public override void StartInteraction(UnitController source, UnitController target)
    {
        var units = new List<UnitController>
            {
                source,
                target,
            };

        foreach (var _target in source.Dialogue.Targets)
        {
            units.Add(_target);
        }

        var origin = GetMidpoint(source);

        UnitController.ArrangeUnitsInRadius(origin, units);
        dialogueReference.StartDialogue(source.Dialogue.GreetingDialogue.Dialogue);
    }

    private Vector3 GetMidpoint(UnitController unit)
    {
        if (unit == null) return Vector3.zero;

        var positions = new List<Vector3> { unit.transform.position };

        foreach (var target in unit.Dialogue.Targets)
        {
            if (target != null)
                positions.Add(target.transform.position);
        }

        if (positions.Count == 0)
            return Vector3.zero;

        // Average all positions
        Vector3 sum = Vector3.zero;
        foreach (var pos in positions)
            sum += pos;

        return sum / positions.Count;
    }
}
