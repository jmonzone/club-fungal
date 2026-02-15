using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine.Events;

public class ObjectPool<T> where T : Component
{
    private T prefab;
    private Transform parent;
    private Queue<T> pool = new Queue<T>();

    public ObjectPool(T prefab, int initialSize = 10, Transform parent = null, UnityAction<T> initialize = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        // Pre-instantiate objects
        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
            initialize?.Invoke(obj);
        }
    }

    public T Get()
    {
        if (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            T obj = Object.Instantiate(prefab, parent);
            return obj;
        }
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}


public enum SporeEmissionBehaviour
{
    UnitSpore,
    Direct,
    Target,
    Fall,
}

public class PlantSporeEmitter : MonoBehaviour, IInteractable, INoteTarget, IForageTarget
{
    [Header("References")]
    [SerializeField] private SporeController sporePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private DJTableReference dJTableReference;
    [SerializeField] private Transform scaleTransform;
    [SerializeField] private ItemTemplate sporeItem;
    [SerializeField] private NetworkRunService networkRunService;
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private SporeEmissionBehaviour emissionBehaviour = SporeEmissionBehaviour.UnitSpore;
    [SerializeField] private int emissionStep = 8;
    [SerializeField] private float launchHeight = 2f;
    [SerializeField] private float bounceScale = 1.2f;
    [SerializeField] private float bounceDuration = 0.3f;

    public int EmissionStep => emissionStep;
    Transform ITarget.Transform => transform;

    void IForageTarget.OnForaged(UnitController forager)
    {
        Select(forager);
    }

    private Vector3 startScale;
    private ObjectPool<SporeController> sporePool;
    private UnityAction onSporeReachedTarget;

    private void Awake()
    {
        startScale = scaleTransform.localScale;
        sporePool = new ObjectPool<SporeController>(sporePrefab, 10, transform, spore =>
        {
            spore.OnCollect += () => sporePool.Return(spore);
        });
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
                    networkRunService.Inventory.AddItem(sporeItem, 1);
                    spore.gameObject.SetActive(false);
                });
                break;

            case SporeEmissionBehaviour.Target:
                EmitSpore(null, () => onSporeReachedTarget?.Invoke());
                break;
        }
    }

    public void SetTarget(Transform newTarget, UnityAction onSporeReachedTarget = null)
    {
        target = newTarget;
        this.onSporeReachedTarget = onSporeReachedTarget;
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
