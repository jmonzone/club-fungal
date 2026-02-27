using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "UnitSpawnInstruction", menuName = "Club Fungal/Units/Unit Spawn Instruction")]
public class UnitSpawnInstruction : ScriptableObject
{
    [Header("Unit Configuration")]
    [SerializeField] private UnitController prefab;
    [SerializeField] private UnitSpecies species;
    [SerializeField] private int spawnCount = 1;

    [Header("NavMesh Settings")]
    [NavMeshArea]
    [SerializeField] private int navMeshArea = -1;

    public UnitController Prefab => prefab;
    public UnitSpecies Species => species;
    public int SpawnCount => spawnCount;
    public int NavMeshArea => navMeshArea;
}
