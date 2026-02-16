using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


public enum SporeEmissionBehaviour
{
    UnitSpore,
    Direct,
    Target,
}

public class PlantSporeEmitter : MonoBehaviour, IInteractable, INoteTarget, IForageTarget
{
    [Header("References")]
    [SerializeField] private SporeController sporePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private InventoryReference inventoryReference;
    [SerializeField] private Item sporeItem;
    [SerializeField] private Transform scaleTransform;
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private SporeEmissionBehaviour emissionBehaviour = SporeEmissionBehaviour.UnitSpore;
    [SerializeField] private int emissionStep = 8;
    [SerializeField] private float launchHeight = 2f;
    [SerializeField] private float bounceScale = 1.2f;
    [SerializeField] private float bounceDuration = 0.3f;
    [SerializeField] private float forageDuration = 2f;

    public int EmissionStep => emissionStep;
    Transform ITarget.Transform => transform;

    public event UnityAction OnMushroomForaged;
    public event UnityAction<float> OnForageProgress;
    public event UnityAction OnForageStarted;
    public event UnityAction OnForageCancelled;

    void IForageTarget.OnForaged(UnitController forager)
    {
        Select(forager);
    }

    private Vector3 startScale;
    private ObjectPool<SporeController> sporePool;
    private UnityAction onSporeReachedTarget;
    private Coroutine forageCoroutine;
    private Dictionary<UnitController, float> activeForagers = new Dictionary<UnitController, float>();
    private float baseForageDuration;

    private void Awake()
    {
        startScale = scaleTransform.localScale;
        sporePool = new ObjectPool<SporeController>(sporePrefab, 10, transform, spore =>
        {
            spore.OnCollect += () => sporePool.Return(spore);
        });
    }

    private void OnEnable()
    {
        // Reset state when enabled (e.g., when retrieved from pool)
        forageCoroutine = null;
        activeForagers.Clear();
        baseForageDuration = forageDuration;

        if (scaleTransform != null && startScale != Vector3.zero)
        {
            scaleTransform.localScale = startScale;
        }
    }

    private void OnDisable()
    {
        CancelForage();
    }

    public void Select(UnitController source)
    {
        switch (emissionBehaviour)
        {
            case SporeEmissionBehaviour.UnitSpore:
                if (source)
                {
                    UnitSpore sporeBehaviour = source.GetComponent<UnitSpore>();
                    sporeBehaviour.SetEmitter(this);
                    source.SetBehaviour(sporeBehaviour);
                }
                break;

            case SporeEmissionBehaviour.Direct:
                EmitSpore(spore =>
                {
                    spore.gameObject.SetActive(false);
                });
                break;

            case SporeEmissionBehaviour.Target:
                // Foraging is now handled by UnitForage calling StartForage directly
                break;
        }
    }

    public void SetTarget(Transform newTarget, UnityAction onSporeReachedTarget = null)
    {
        target = newTarget;
        this.onSporeReachedTarget = onSporeReachedTarget;
    }

    public void StartForage(UnitController forager, float speedMultiplier = 1f)
    {
        // Add or update this forager's contribution
        if (!activeForagers.ContainsKey(forager))
        {
            activeForagers[forager] = speedMultiplier;
        }
        else
        {
            activeForagers[forager] = speedMultiplier;
        }

        // Start foraging if not already started
        if (forageCoroutine == null)
        {
            forageCoroutine = StartCoroutine(ForageRoutine());
        }
    }

    public void CancelForage()
    {
        if (forageCoroutine != null)
        {
            StopCoroutine(forageCoroutine);
            forageCoroutine = null;
        }

        if (activeForagers.Count > 0)
        {
            activeForagers.Clear();
            OnForageCancelled?.Invoke();
            OnForageProgress?.Invoke(0f);
        }
    }

    private IEnumerator ForageRoutine()
    {
        OnForageStarted?.Invoke();
        float accumulatedProgress = 0f;
        bool wasCancelled = false;

        while (true)
        {
            // Remove invalid or distant foragers
            var foragersToRemove = new List<UnitController>();
            foreach (var kvp in activeForagers)
            {
                UnitController forager = kvp.Key;
                if (forager == null || !forager.gameObject.activeInHierarchy)
                {
                    foragersToRemove.Add(forager);
                    continue;
                }

                float distance = Vector3.Distance(transform.position, forager.transform.position);
                if (distance > 2f) // Cancel if unit moves too far away
                {
                    foragersToRemove.Add(forager);
                }
            }

            foreach (var forager in foragersToRemove)
            {
                activeForagers.Remove(forager);
            }

            // If no valid foragers remain, cancel
            if (activeForagers.Count == 0)
            {
                wasCancelled = true;
                break;
            }

            // Calculate combined speed multiplier (sum of all foragers' multipliers)
            float combinedSpeedMultiplier = 0f;
            foreach (var speedMultiplier in activeForagers.Values)
            {
                combinedSpeedMultiplier += speedMultiplier;
            }

            // Accumulate progress based on combined effort this frame
            // More/faster foragers = more progress per frame
            accumulatedProgress += Time.deltaTime * combinedSpeedMultiplier;

            // Calculate progress as ratio of work done vs total work needed
            float progress = Mathf.Clamp01(accumulatedProgress / baseForageDuration);
            OnForageProgress?.Invoke(progress);

            // Check if forage is complete
            if (accumulatedProgress >= baseForageDuration)
            {
                break;
            }

            yield return null;
        }

        if (wasCancelled)
        {
            // Clean up cancelled forage
            forageCoroutine = null;
            activeForagers.Clear();
            OnForageCancelled?.Invoke();
            OnForageProgress?.Invoke(0f);
        }
        else
        {
            // Forage complete
            OnForageProgress?.Invoke(1f);
            inventoryReference.AddItem(sporeItem);
            OnMushroomForaged?.Invoke();
            gameObject.SetActive(false);
            forageCoroutine = null;
            activeForagers.Clear();
        }
    }

    public void EmitSpore(UnityAction<SporeController> onPeakAction = null, UnityAction onLandAction = null)
    {
        StopAllCoroutines();
        StartCoroutine(BounceAnimation());

        SporeController spore = sporePool.Get();
        spore.transform.position = spawnPoint.position;
        spore.transform.rotation = Quaternion.identity;

        Vector3 peak = spawnPoint.position + Vector3.up * launchHeight;
        Vector3 landingSpot = target ? target.position : FindLandingSpot();

        spore.LaunchSpore(peak, landingSpot, () => onPeakAction?.Invoke(spore), onLandAction);
    }

    private IEnumerator BounceAnimation()
    {
        scaleTransform.localScale = startScale;

        Vector3 targetScale = startScale * bounceScale;
        float t = 0f;

        // Scale up
        while (t < bounceDuration)
        {
            t += Time.deltaTime;
            float progress = t / bounceDuration;
            scaleTransform.localScale = Vector3.Lerp(startScale, targetScale, Mathf.Sin(progress * Mathf.PI));
            yield return null;
        }

        scaleTransform.localScale = startScale;
    }

    private Vector3 FindLandingSpot()
    {
        Vector3 randomDirection = Random.insideUnitCircle.normalized;
        randomDirection.z = randomDirection.y;
        randomDirection.y = 0;
        randomDirection *= Random.Range(1.25f, 1.75f);

        Vector3 targetPos = transform.position + randomDirection;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        Debug.Log("fallback");
        // fallback: just drop next to plant
        return transform.position + (Vector3.right * 1f);
    }

    void INoteTarget.OnHit(DJTrack track)
    {
        EmitSpore(null, () => onSporeReachedTarget?.Invoke());
    }
}
