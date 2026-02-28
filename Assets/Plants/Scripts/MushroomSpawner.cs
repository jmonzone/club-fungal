using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public struct MushroomSpawnData
{
    public PlantSporeEmitter prefab;
    [Range(0f, 1f)]
    [Tooltip("Spawn chance (0-1). All weights are normalized to sum to 1.")]
    public float spawnWeight;
}

public class MushroomSpawner : NavMeshSpawner
{
    [Header("Mushroom Settings")]
    [SerializeField] private MushroomSpawnData[] mushroomTypes;
    [SerializeField] private Transform mushroomParent;

    [Header("Randomization")]
    [SerializeField] private bool randomizeRotation = true;
    [SerializeField] private bool randomizeScale = false;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    [Header("Runtime")]
    [SerializeField] private List<GameObject> spawnedMushrooms = new List<GameObject>();

    private Dictionary<PlantSporeEmitter, ObjectPool<PlantSporeEmitter>> mushroomPools = new Dictionary<PlantSporeEmitter, ObjectPool<PlantSporeEmitter>>();
    private Dictionary<GameObject, Vector3> mushroomPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, UnityAction> mushroomCallbacks = new Dictionary<GameObject, UnityAction>();
    private float totalSpawnWeight;

    protected override void Awake()
    {
        // Normalize weights to ensure they sum to 1
        NormalizeWeights();

        // Initialize pools for each mushroom type
        totalSpawnWeight = 0f;
        foreach (var mushroomType in mushroomTypes)
        {
            if (mushroomType.prefab != null)
            {
                mushroomPools[mushroomType.prefab] = new ObjectPool<PlantSporeEmitter>(
                    mushroomType.prefab,
                    spawnCount * 2,
                    transform,
                    mushroom => { /* Forage callback handled in spawning */ }
                );
                totalSpawnWeight += mushroomType.spawnWeight;
            }
        }

        base.Awake();
    }

    public override void Spawn()
    {
        SpawnMushrooms();
    }

    public void SpawnMushrooms()
    {
        ClearMushrooms();

        if (mushroomTypes == null || mushroomTypes.Length == 0)
        {
            Debug.LogWarning("MushroomSpawner: No mushroom types assigned");
            return;
        }

        // if (spawnCollider == null)
        // {
        //     Debug.LogWarning("MushroomSpawner: No spawn collider assigned");
        //     return;
        // }

        List<Vector3> allPositions = GenerateSpawnPositions(spawnCount);
        availableSpawnPositions = new List<Vector3>(allPositions);
        occupiedPositions.Clear();

        for (int i = 0; i < Mathf.Min(spawnCount, availableSpawnPositions.Count); i++)
        {
            SpawnMushroomAtRandomAvailablePosition();
        }

        Debug.Log($"MushroomSpawner: Spawned {spawnedMushrooms.Count} mushrooms");
    }

    private void SpawnMushroomAtRandomAvailablePosition()
    {
        Vector3 position = GetRandomAvailablePosition();
        if (position == Vector3.zero) return;

        SpawnMushroomAt(position);
    }

    private void SpawnMushroomAt(Vector3 position)
    {
        // Select mushroom type based on spawn weights
        PlantSporeEmitter selectedPrefab = SelectRandomMushroomType();
        if (selectedPrefab == null || !mushroomPools.ContainsKey(selectedPrefab))
        {
            Debug.LogWarning("MushroomSpawner: Could not select valid mushroom type");
            return;
        }

        PlantSporeEmitter mushroom = mushroomPools[selectedPrefab].Get();
        mushroom.transform.position = position;

        if (randomizeRotation)
        {
            float randomY = Random.Range(0f, 360f);
            mushroom.transform.rotation = Quaternion.Euler(0f, randomY, 0f);
        }

        if (randomizeScale)
        {
            float randomScale = Random.Range(scaleRange.x, scaleRange.y);
            mushroom.transform.localScale = Vector3.one * randomScale;
        }

        // Subscribe to the foraged event
        GameObject mushroomObj = mushroom.gameObject;
        UnityAction callback = () => OnMushroomForaged(mushroomObj, selectedPrefab);
        mushroomCallbacks[mushroomObj] = callback;
        mushroom.OnMushroomForaged += callback;

        // Track position
        mushroomPositions[mushroomObj] = position;

        spawnedMushrooms.Add(mushroomObj);
    }

    private PlantSporeEmitter SelectRandomMushroomType()
    {
        if (totalSpawnWeight <= 0f) return null;

        float randomValue = Random.Range(0f, totalSpawnWeight);
        float cumulativeWeight = 0f;

        foreach (var mushroomType in mushroomTypes)
        {
            if (mushroomType.prefab == null) continue;

            cumulativeWeight += mushroomType.spawnWeight;
            if (randomValue <= cumulativeWeight)
            {
                return mushroomType.prefab;
            }
        }

        // Fallback to first valid prefab
        foreach (var mushroomType in mushroomTypes)
        {
            if (mushroomType.prefab != null) return mushroomType.prefab;
        }
        return null;
    }

    private void OnMushroomForaged(GameObject mushroom, PlantSporeEmitter prefabType)
    {
        if (spawnedMushrooms.Contains(mushroom))
        {
            spawnedMushrooms.Remove(mushroom);

            // Free up the position
            if (mushroomPositions.TryGetValue(mushroom, out Vector3 oldPosition))
            {
                FreePosition(oldPosition);
                mushroomPositions.Remove(mushroom);
            }

            // Unsubscribe from event before returning to pool
            var plantEmitter = mushroom.GetComponent<PlantSporeEmitter>();
            if (plantEmitter != null && mushroomCallbacks.TryGetValue(mushroom, out UnityAction callback))
            {
                plantEmitter.OnMushroomForaged -= callback;
                mushroomCallbacks.Remove(mushroom);
            }

            if (mushroomPools.ContainsKey(prefabType))
            {
                mushroomPools[prefabType].Return(plantEmitter);
            }

            // Spawn a new mushroom at a random available position
            SpawnMushroomAtRandomAvailablePosition();
        }
    }

    public void ClearMushrooms()
    {
        foreach (GameObject mushroom in spawnedMushrooms)
        {
            if (mushroom != null)
            {
                // Unsubscribe from events
                var plantEmitter = mushroom.GetComponent<PlantSporeEmitter>();
                if (plantEmitter != null && mushroomCallbacks.TryGetValue(mushroom, out UnityAction callback))
                {
                    plantEmitter.OnMushroomForaged -= callback;
                }

                // Find which pool this mushroom belongs to
                foreach (var pool in mushroomPools)
                {
                    if (plantEmitter != null && plantEmitter.name.Contains(pool.Key.name))
                    {
                        pool.Value.Return(plantEmitter);
                        break;
                    }
                }
            }
        }
        spawnedMushrooms.Clear();
        mushroomCallbacks.Clear();
        mushroomPositions.Clear();
    }

    private void NormalizeWeights()
    {
        if (mushroomTypes == null || mushroomTypes.Length == 0) return;

        float total = 0f;
        for (int i = 0; i < mushroomTypes.Length; i++)
        {
            if (mushroomTypes[i].prefab != null)
            {
                total += mushroomTypes[i].spawnWeight;
            }
        }

        // Only normalize if weights don't sum to approximately 1
        if (total > 0 && Mathf.Abs(total - 1f) > 0.01f)
        {
            for (int i = 0; i < mushroomTypes.Length; i++)
            {
                if (mushroomTypes[i].prefab != null)
                {
                    var data = mushroomTypes[i];
                    data.spawnWeight /= total;
                    mushroomTypes[i] = data;
                }
            }
        }
    }

    protected override Color GetGizmoColor()
    {
        return new Color(0f, 1f, 0f, 0.3f);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (spawnedMushrooms != null)
        {
            Gizmos.color = Color.yellow;
            foreach (GameObject mushroom in spawnedMushrooms)
            {
                if (mushroom != null)
                {
                    Gizmos.DrawWireSphere(mushroom.transform.position, minSpacing * 0.5f);
                }
            }
        }
    }
}