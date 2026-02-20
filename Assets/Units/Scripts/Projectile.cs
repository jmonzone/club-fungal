using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float arcHeight = 2f;
    [SerializeField] private float spiralRadius = 0.3f;
    [SerializeField] private float spiralSpeed = 3f;

    [Header("Visual Settings")]
    [SerializeField] private float initialScale = 1f;
    [SerializeField] private float endScale = 0.5f;

    private Transform target;
    private UnitController targetController;
    private float damage;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float startTime;
    private float journeyLength;
    private float spiralPhaseOffset;
    private float spiralRadiusOffset;

    public event UnityAction OnHit;

    private void Awake()
    {
        // Random offsets for visual variety
        spiralPhaseOffset = Random.Range(0f, Mathf.PI * 2f);
        spiralRadiusOffset = Random.Range(0.8f, 1.2f);
    }

    public void Initialize(Transform target)
    {
        this.target = target;
        this.targetController = null;
        this.damage = 0f;
        startPos = transform.position;
        startTime = Time.time;
        transform.localScale = Vector3.one * initialScale;
    }

    public void Initialize(UnitController targetController, float damage)
    {
        this.target = targetController.transform;
        this.targetController = targetController;
        this.damage = damage;
        startPos = transform.position;
        startTime = Time.time;
        transform.localScale = Vector3.one * initialScale;
    }

    private void Update()
    {
        if (target == null)
        {
            OnHitTarget();
            return;
        }

        targetPos = target.position;
        journeyLength = 2f;

        // Calculate fraction of journey (0..1)
        float distCovered = (Time.time - startTime) * speed;
        float fracJourney = Mathf.Clamp01(distCovered / journeyLength);

        // Base position along trajectory
        Vector3 newPos = Vector3.Lerp(startPos, targetPos, fracJourney);

        // Add parabolic arc
        float arc = arcHeight * Mathf.Sin(fracJourney * Mathf.PI);
        newPos.y += arc;

        // Add spiral offset
        float angle = fracJourney * spiralSpeed * Mathf.PI * 2f + spiralPhaseOffset;
        Vector3 direction = (targetPos - startPos).normalized;
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, direction).normalized;
        Vector3 spiralOffset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up)
            * spiralRadius * spiralRadiusOffset * (1 - fracJourney);
        newPos += spiralOffset;

        transform.position = newPos;

        // Rotate to face movement direction
        if (fracJourney > 0)
        {
            Vector3 lookDirection = (newPos - transform.position).normalized;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.forward = lookDirection;
            }
        }

        // Scale down as it approaches target
        float scale = Mathf.Lerp(initialScale, endScale, fracJourney);
        transform.localScale = Vector3.one * scale;

        // Hit target when close enough
        if (fracJourney >= 0.9f)
        {
            OnHitTarget();
        }
    }

    private void OnHitTarget()
    {
        // Deal damage to target if it has health
        if (targetController != null && damage > 0f)
        {
            UnitHealth health = targetController.Health;
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        // Notify pool to return this projectile
        OnHit?.Invoke();
        gameObject.SetActive(false);
    }
}
