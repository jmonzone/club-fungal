using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : NavMeshSpawner
{
    [Header("Unit Settings")]
    [SerializeField] private List<UnitSpawnInstruction> instructions;
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private float respawnDelay = 10f;

    [Header("Runtime")]
    [SerializeField] private List<UnitController> spawnedUnits = new List<UnitController>();

    public override void Spawn()
    {
        SpawnUnits();
    }

    public void SpawnUnits()
    {
        List<Vector3> allPositions = GenerateSpawnPositions(spawnCount * instructions.Count);
        availableSpawnPositions = new List<Vector3>(allPositions);
        occupiedPositions.Clear();

        // Spawn units from each instruction
        foreach (UnitSpawnInstruction instruction in instructions)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnUnitAtRandomAvailablePosition(instruction);
            }
        }

        Debug.Log($"UnitSpawner: Spawned {spawnedUnits.Count} units");
    }

    private void SpawnUnitAtRandomAvailablePosition(UnitSpawnInstruction instruction)
    {
        Vector3 position = GetRandomAvailablePosition();
        if (position == Vector3.zero) return;

        // Create UnitInstance (not registered so it won't be persisted)
        UnitInstance unitInstance = null;
        if (instruction.Species != null)
        {
            unitInstance = unitInstanceService.CreateUnit(species => species == instruction.Species, register: false);
        }
        else
        {
            // Fallback to random unit if no species specified
            unitInstance = unitInstanceService.CreateUnit(register: false);
        }

        UnitController unit = unitControllerService.SpawnUnit(unitInstance, position, null, instruction.Prefab);

        spawnedUnits.Add(unit);
    }

    public void ClearUnits()
    {
        foreach (UnitController unit in spawnedUnits)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }
        spawnedUnits.Clear();
    }

    protected override Color GetGizmoColor()
    {
        return new Color(1f, 1f, 1f, 0.3f);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (spawnedUnits != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            foreach (UnitController unit in spawnedUnits)
            {
                if (unit != null)
                {
                    Gizmos.DrawWireSphere(unit.transform.position, minSpacing * 0.5f);
                }
            }
        }
    }
}
