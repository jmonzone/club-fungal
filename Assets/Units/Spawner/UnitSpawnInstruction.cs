using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "UnitSpawnInstruction", menuName = "Club Fungal/Units/Unit Spawn Instruction")]
public class UnitSpawnInstruction : ScriptableObject
{
    [Header("Unit Configuration")]
    [SerializeField] private UnitController prefab;
    [SerializeField] private List<UnitSpecies> species = new List<UnitSpecies>();
    [SerializeField] private int spawnCount = 1;

    [Header("NavMesh Settings")]
    [NavMeshArea]
    [SerializeField] private int navMeshArea = -1;

    public UnitController Prefab => prefab;
    public List<UnitSpecies> Species => species;
    public int SpawnCount => spawnCount;
    public int NavMeshArea => navMeshArea;
}
