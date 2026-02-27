using UnityEngine;

[CreateAssetMenu(fileName = "UnitSpawnInstruction", menuName = "Club Fungal/Units/Unit Spawn Instruction")]
public class UnitSpawnInstruction : ScriptableObject
{
    [Header("Unit Configuration")]
    [SerializeField] private UnitController prefab;
    [SerializeField] private UnitSpecies species;

    public UnitController Prefab => prefab;
    public UnitSpecies Species => species;
}
