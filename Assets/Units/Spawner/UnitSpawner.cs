using UnityEngine;
using UnityEngine.AI;
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
        availableSpawnPositions = new List<Vector3>();
        occupiedPositions.Clear();

        // Spawn units from each instruction
        foreach (UnitSpawnInstruction instruction in instructions)
        {
            // Convert area index to area mask
            int areaMask = instruction.NavMeshArea == -1
                ? NavMesh.AllAreas
                : (1 << instruction.NavMeshArea);

            Debug.Log($"Spawning {instruction.name}: count={instruction.SpawnCount}, area={instruction.NavMeshArea}, mask={areaMask}");

            List<Vector3> positions = GenerateSpawnPositionsWithAreaMask(instruction.SpawnCount, areaMask);

            foreach (Vector3 position in positions)
            {
                SpawnUnitAtPosition(instruction, position);
            }
        }

        Debug.Log($"UnitSpawner: Spawned {spawnedUnits.Count} units");
    }

    private List<Vector3> GenerateSpawnPositionsWithAreaMask(int count, int areaMask)
    {
        List<Vector3> positions = new List<Vector3>();

        // Get all NavMesh triangulation data to sample from entire surface
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        if (triangulation.vertices.Length == 0)
        {
            Debug.LogWarning($"UnitSpawner: No NavMesh found");
            return positions;
        }

        // Calculate bounds from all NavMesh vertices
        Bounds bounds = new Bounds(triangulation.vertices[0], Vector3.zero);
        foreach (Vector3 vertex in triangulation.vertices)
        {
            bounds.Encapsulate(vertex);
        }

        int maxAttempts = count * 10;
        int attempts = 0;

        while (positions.Count < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Sample NavMesh with specified area mask
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit navHit, 10f, areaMask))
            {
                Vector3 finalPosition = navHit.position;

                // Apply edge padding to avoid spawning on edges
                if (navMeshAreaConfig != null && navMeshAreaConfig.edgePadding > 0f)
                {
                    finalPosition = ApplyEdgePadding(finalPosition, areaMask);
                }

                // Check spacing from other positions
                if (IsValidSpacing(finalPosition, positions))
                {
                    positions.Add(finalPosition);
                }
            }
        }

        Debug.Log($"Generated {positions.Count} positions out of {count} requested (areaMask={areaMask})");
        return positions;
    }

    private Vector3 ApplyEdgePadding(Vector3 position, int areaMask)
    {
        float edgePadding = navMeshAreaConfig.edgePadding;

        // Sample 8 directions to find which one has the most continuous target area
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized
        };

        int bestScore = -1;
        Vector3 bestDirection = Vector3.zero;

        foreach (Vector3 dir in directions)
        {
            int score = 0;
            // Check multiple distances in this direction
            for (float dist = edgePadding; dist <= edgePadding * 3f; dist += edgePadding)
            {
                Vector3 testPos = position + dir * dist;
                if (NavMesh.SamplePosition(testPos, out NavMeshHit hit, 0.5f, areaMask))
                {
                    score++;
                }
                else
                {
                    break; // Stop checking this direction if we hit non-target area
                }
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = dir;
            }
        }

        // If we found a good direction, pad in that direction
        if (bestScore > 0)
        {
            Vector3 paddedPosition = position + bestDirection * edgePadding;
            if (NavMesh.SamplePosition(paddedPosition, out NavMeshHit paddedHit, 0.5f, areaMask))
            {
                return paddedHit.position;
            }
        }

        return position;
    }

    private void SpawnUnitAtPosition(UnitSpawnInstruction instruction, Vector3 position)
    {
        // Create UnitInstance (not registered so it won't be persisted)
        UnitInstance unitInstance = null;
        if (instruction.Species != null && instruction.Species.Count > 0)
        {
            // Randomly select a species from the list
            UnitSpecies selectedSpecies = instruction.Species[Random.Range(0, instruction.Species.Count)];
            unitInstance = unitInstanceService.CreateUnit(species => species == selectedSpecies, register: false);
        }
        else
        {
            // Fallback to random unit if no species specified
            unitInstance = unitInstanceService.CreateUnit(register: false);
        }

        Transform parent = networkRunService != null && networkRunService.PartyService != null
            ? networkRunService.PartyService.SpawnParent
            : null;

        UnitController unit = unitControllerService.SpawnUnit(unitInstance, position, parent, instruction.Prefab);

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
