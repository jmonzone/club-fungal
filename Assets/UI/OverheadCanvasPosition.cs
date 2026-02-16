using UnityEngine;

public class OverheadCanvasPosition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private bool hideWhenOffscreen = true;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private bool hideWhenTooFar = false;

    private Canvas canvas;

    private void Awake()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        canvas = GetComponentInParent<Canvas>();
    }

    private void LateUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (targetTransform == null || mainCamera == null || rectTransform == null)
        {
            return;
        }

        // Calculate world position with offset
        Vector3 worldPosition = targetTransform.position + worldOffset;

        // Check distance
        float distance = Vector3.Distance(mainCamera.transform.position, worldPosition);
        if (hideWhenTooFar && distance > maxDistance)
        {
            return;
        }

        // Convert world position to screen point
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPosition);

        // Check if behind camera or off screen
        if (screenPoint.z < 0)
        {
            return;
        }

        // Convert screen point to canvas position
        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                rectTransform.position = screenPoint;
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                Vector2 canvasPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPoint,
                    canvas.worldCamera,
                    out canvasPosition
                );
                rectTransform.localPosition = canvasPosition;
            }
        }
        else
        {
            // Fallback for direct screen space
            rectTransform.position = screenPoint;
        }
    }

    private void OnValidate()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }
}
