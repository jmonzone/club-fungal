using UnityEngine;
using System.Collections.Generic;

public class NeutralUnitSpawner : NavMeshSpawner
{
    [Header("Neutral Unit Settings")]
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private UnitController neutralPrefab;
    [SerializeField] private UnitSpecies neutralSpecies;
    [SerializeField] private Transform neutralParent;
    [SerializeField] private float respawnDelay = 10f;

    [Header("Runtime")]
    [SerializeField] private List<UnitController> spawnedUnits = new List<UnitController>();

    public override void Spawn()
    {
        SpawnNeutralUnits();
    }

    public void SpawnNeutralUnits()
    {
        if (unitControllerService == null)
        {
            Debug.LogWarning("NeutralUnitSpawner: UnitControllerService not assigned");
            return;
        }

        if (spawnCollider == null)
        {
            Debug.LogWarning("NeutralUnitSpawner: No spawn collider assigned");
            return;
        }

        List<Vector3> allPositions = GenerateSpawnPositions(spawnCount);
        availableSpawnPositions = new List<Vector3>(allPositions);
        occupiedPositions.Clear();

        for (int i = 0; i < Mathf.Min(spawnCount, availableSpawnPositions.Count); i++)
        {
            SpawnNeutralUnitAtRandomAvailablePosition();
        }

        Debug.Log($"NeutralUnitSpawner: Spawned {spawnedUnits.Count} neutral units");
    }

    private void SpawnNeutralUnitAtRandomAvailablePosition()
    {
        Vector3 position = GetRandomAvailablePosition();
        if (position == Vector3.zero) return;

        if (neutralPrefab == null)
        {
            Debug.LogWarning("NeutralUnitSpawner: No neutral prefab assigned");
            return;
        }

        // Create UnitInstance (marked as enemy in data so it won't be persisted)
        UnitInstance neutralInstance = null;
        if (neutralSpecies != null)
        {
            neutralInstance = unitInstanceService.CreateUnit(species => species == neutralSpecies, isEnemy: true);
        }
        else
        {
            // Fallback to random unit if no neutral species specified
            neutralInstance = unitInstanceService.CreateUnit(isEnemy: true);
        }

        Transform parent = neutralParent != null ? neutralParent : (networkRunService != null && networkRunService.PartyService != null ? networkRunService.PartyService.SpawnParent : null);

        UnitController neutralUnit = unitControllerService.SpawnUnit(neutralInstance, position, parent, neutralPrefab);

        if (neutralUnit == null)
        {
            Debug.LogWarning("NeutralUnitSpawner: Failed to spawn neutral unit");
            return;
        }

        // Don't call SetAsEnemy - we want normal naming and behavior, just no persistence
        spawnedUnits.Add(neutralUnit);
    }

    public void ClearNeutralUnits()
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
        return new Color(0.5f, 0.5f, 1f, 0.3f); // Blue tint for neutral units
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (spawnedUnits != null)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.8f);
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
