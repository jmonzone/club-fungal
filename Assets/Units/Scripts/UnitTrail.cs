using UnityEngine;

public class UnitTrail : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem trailParticles;
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("Settings")]
    [SerializeField] private Color trailColor = new Color(1f, 0.8f, 0.2f, 1f); // Yellow/gold
    [SerializeField] private bool useParticles = true;

    private void Awake()
    {
        if (trailParticles == null)
        {
            trailParticles = GetComponentInChildren<ParticleSystem>();
        }

        if (trailRenderer == null)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        // Ensure trail starts disabled
        SetTrailActive(false);
    }

    public void ActivateTrail()
    {
        SetTrailActive(true);
    }

    public void DeactivateTrail()
    {
        SetTrailActive(false);
    }

    private void SetTrailActive(bool active)
    {
        if (useParticles && trailParticles != null)
        {
            if (active)
            {
                trailParticles.Play();
            }
            else
            {
                trailParticles.Stop();
            }
        }

        if (trailRenderer != null)
        {
            trailRenderer.enabled = active;
            if (active)
            {
                trailRenderer.startColor = trailColor;
                trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
            }
        }
    }

    public void SetTrailColor(Color color)
    {
        trailColor = color;
        if (trailRenderer != null && trailRenderer.enabled)
        {
            trailRenderer.startColor = color;
            trailRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
        }

        if (trailParticles != null)
        {
            var main = trailParticles.main;
            main.startColor = color;
        }
    }
}
