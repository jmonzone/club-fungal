using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SporeController : MonoBehaviour, IForageTarget
{
    [Header("References")]
    [SerializeField] private InventoryReference inventoryReference;

    [Header("Spore Settings")]
    [SerializeField] private float driftDownDuration = 2f;

    public event UnityAction OnCollect;

    // ITarget & IForageTarget implementation
    Transform ITarget.Transform => transform;
    bool IForageTarget.IsAvailable => gameObject.activeInHierarchy;

    void IForageTarget.OnForaged(UnitController forager)
    {
        Collect();
    }

    public void Collect()
    {
        inventoryReference.CollectSpore(this);
        StopAllCoroutines();
        gameObject.SetActive(false);
        OnCollect?.Invoke();
    }

    public void LaunchSpore(Vector3 peak, Vector3 landing, UnityAction onPeakAction = null, UnityAction onLandAction = null)
    {
        StartCoroutine(AnimateSpore(peak, landing, onPeakAction, onLandAction));
    }

    private IEnumerator AnimateSpore(Vector3 peak, Vector3 landing, UnityAction onPeakAction = null, UnityAction onLandAction = null)
    {
        Vector3 start = transform.position;
        float t = 0;

        var launchDuration = 0.5f;
        peak.x += Mathf.Sin(Random.value * Mathf.PI * 2f) * 0.01f; // feather sway
        peak.z += Mathf.Cos(Random.value * Mathf.PI * 2f) * 0.01f; // feather sway

        // Go upward
        while (t < launchDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, peak, t / launchDuration);
            yield return null;
        }

        transform.position = peak;
        onPeakAction?.Invoke();

        // Drift down with side sway like feather
        t = 0;
        while (t < driftDownDuration)
        {
            t += Time.deltaTime;
            float progress = t / driftDownDuration;

            Vector3 pos = Vector3.Lerp(peak, landing, progress);
            pos.x += Mathf.Sin(progress * Mathf.PI * 2f) * 0.2f; // feather sway
            transform.position = pos;

            yield return null;
        }

        transform.position = landing;
        onLandAction?.Invoke();
    }
}
