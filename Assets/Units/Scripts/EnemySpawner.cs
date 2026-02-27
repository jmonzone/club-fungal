using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : NavMeshSpawner
{
    [Header("Enemy Settings")]
    [SerializeField] private UnitInstanceService unitInstanceService;
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private UnitControllerService unitControllerService;
    [SerializeField] private UnitController enemyPrefab;
    [SerializeField] private UnitSpecies enemySpecies;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private float respawnDelay = 10f;

    [Header("Runtime")]
    [SerializeField] private List<UnitController> spawnedEnemies = new List<UnitController>();

    public override void Spawn()
    {
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        if (unitControllerService == null)
        {
            Debug.LogWarning("EnemySpawner: UnitControllerService not assigned");
            return;
        }

        if (spawnCollider == null)
        {
            Debug.LogWarning("EnemySpawner: No spawn collider assigned");
            return;
        }

        List<Vector3> allPositions = GenerateSpawnPositions(spawnCount);
        availableSpawnPositions = new List<Vector3>(allPositions);
        occupiedPositions.Clear();

        for (int i = 0; i < Mathf.Min(spawnCount, availableSpawnPositions.Count); i++)
        {
            SpawnEnemyAtRandomAvailablePosition();
        }

        Debug.Log($"EnemySpawner: Spawned {spawnedEnemies.Count} enemies");
    }

    private void SpawnEnemyAtRandomAvailablePosition()
    {
        Vector3 position = GetRandomAvailablePosition();
        if (position == Vector3.zero) return;

        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefab assigned");
            return;
        }

        // Create UnitInstance for enemy (marked as enemy so it won't be persisted)
        UnitInstance enemyInstance = null;
        if (enemySpecies != null)
        {
            enemyInstance = unitInstanceService.CreateUnit(species => species == enemySpecies, isEnemy: true);
        }
        else
        {
            // Fallback to random unit if no enemy species specified
            enemyInstance = unitInstanceService.CreateUnit(isEnemy: true);
        }

        Transform parent = enemyParent != null ? enemyParent : (networkRunService != null && networkRunService.PartyService != null ? networkRunService.PartyService.SpawnParent : null);

        UnitController enemy = unitControllerService.SpawnUnit(enemyInstance, position, parent, enemyPrefab);

        if (enemy == null)
        {
            Debug.LogWarning("EnemySpawner: Failed to spawn enemy");
            return;
        }

        enemy.SetAsEnemy(true);
        spawnedEnemies.Add(enemy);
    }

    public void ClearEnemies()
    {
        foreach (UnitController enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }
        spawnedEnemies.Clear();
    }

    protected override Color GetGizmoColor()
    {
        return new Color(1f, 0f, 0f, 0.3f);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (spawnedEnemies != null)
        {
            Gizmos.color = Color.red;
            foreach (UnitController enemy in spawnedEnemies)
            {
                if (enemy != null)
                {
                    Gizmos.DrawWireSphere(enemy.transform.position, minSpacing * 0.5f);
                }
            }
        }
    }
}
