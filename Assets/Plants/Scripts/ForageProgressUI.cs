using UnityEngine;
using UnityEngine.UI;

public class ForageProgressUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlantSporeEmitter plantSporeEmitter;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private GameObject progressContainer;

    private void Awake()
    {
        if (plantSporeEmitter == null)
        {
            plantSporeEmitter = GetComponent<PlantSporeEmitter>();
        }

        if (progressContainer != null)
        {
            progressContainer.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // Reset UI state when enabled (e.g., when retrieved from pool)
        if (progressContainer != null)
        {
            progressContainer.SetActive(false);
        }

        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }

        if (plantSporeEmitter != null)
        {
            plantSporeEmitter.OnForageStarted += HandleForageStarted;
            plantSporeEmitter.OnForageProgress += HandleForageProgress;
            plantSporeEmitter.OnForageCancelled += HandleForageCancelled;
            plantSporeEmitter.OnMushroomForaged += HandleForageComplete;
        }
    }

    private void OnDisable()
    {
        if (plantSporeEmitter != null)
        {
            plantSporeEmitter.OnForageStarted -= HandleForageStarted;
            plantSporeEmitter.OnForageProgress -= HandleForageProgress;
            plantSporeEmitter.OnForageCancelled -= HandleForageCancelled;
            plantSporeEmitter.OnMushroomForaged -= HandleForageComplete;
        }
    }

    private void HandleForageStarted()
    {
        if (progressContainer != null)
        {
            progressContainer.SetActive(true);
        }

        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }
    }

    private void HandleForageProgress(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.value = progress;
        }
    }

    private void HandleForageCancelled()
    {
        if (progressContainer != null)
        {
            progressContainer.SetActive(false);
        }

        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }
    }

    private void HandleForageComplete()
    {
        if (progressContainer != null)
        {
            progressContainer.SetActive(false);
        }
    }
}
