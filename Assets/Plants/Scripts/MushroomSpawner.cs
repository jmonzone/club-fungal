using UnityEngine;
using System.Collections.Generic;

public class MushroomSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject mushroomPrefab;
    [SerializeField] private Collider spawnCollider;
    [SerializeField] private Transform mushroomParent;

    [Header("Spawn Settings")]
    [SerializeField] private int mushroomCount = 10;
    [SerializeField] private float minSpacing = 0.5f;
    [SerializeField] private bool spawnOnAwake = false;

    [Header("Randomization")]
    [SerializeField] private bool randomizeRotation = true;
    [SerializeField] private bool randomizeScale = false;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    [Header("Runtime")]
    [SerializeField] private List<GameObject> spawnedMushrooms = new List<GameObject>();

    private void Awake()
    {
        if (spawnOnAwake)
        {
            SpawnMushrooms();
        }
    }

    public void SpawnMushrooms()
    {
        ClearMushrooms();

        if (mushroomPrefab == null)
        {
            Debug.LogWarning("MushroomSpawner: No mushroom prefab assigned");
            return;
        }

        if (spawnCollider == null)
        {
            Debug.LogWarning("MushroomSpawner: No spawn collider assigned");
            return;
        }

        Transform parent = mushroomParent != null ? mushroomParent : transform;
        List<Vector3> spawnPositions = GenerateSpawnPositions();

        foreach (Vector3 position in spawnPositions)
        {
            GameObject mushroom = Instantiate(mushroomPrefab, parent);
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

            spawnedMushrooms.Add(mushroom);
        }

        Debug.Log($"MushroomSpawner: Spawned {spawnedMushrooms.Count} mushrooms");
    }

    private List<Vector3> GenerateSpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        Bounds bounds = spawnCollider.bounds;

        int maxAttempts = mushroomCount * 10;
        int attempts = 0;

        while (positions.Count < mushroomCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Raycast down to find ground
            Vector3 finalPosition = randomPoint;
            if (Physics.Raycast(randomPoint + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                finalPosition = hit.point;
            }
            else
            {
                continue; // Skip if no ground found
            }

            // Check spacing from other mushrooms
            if (IsValidSpacing(finalPosition, positions))
            {
                positions.Add(finalPosition);
            }
        }

        return positions;
    }

    private bool IsValidSpacing(Vector3 position, List<Vector3> existingPositions)
    {
        foreach (Vector3 existing in existingPositions)
        {
            float distance = Vector3.Distance(position, existing);
            if (distance < minSpacing)
            {
                return false;
            }
        }
        return true;
    }

    public void ClearMushrooms()
    {
        foreach (GameObject mushroom in spawnedMushrooms)
        {
            if (mushroom != null)
            {
                Destroy(mushroom);
            }
        }
        spawnedMushrooms.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnCollider != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(spawnCollider.bounds.center, spawnCollider.bounds.size);
        }

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
