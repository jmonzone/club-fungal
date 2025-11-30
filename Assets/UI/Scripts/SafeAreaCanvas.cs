using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class SafeAreaCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float uniformPaddingPixels = 0;

    private Rect lastSafeArea;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        ApplySafeArea();
        lastSafeArea = Screen.safeArea;
    }

    private void OnValidate()
    {
        ApplySafeArea();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Screen.safeArea != lastSafeArea)
        {
            ApplySafeArea();
            lastSafeArea = Screen.safeArea;
        }
    }
#endif

    public void ApplySafeArea()
    {
        Rect canvasRect = canvas.pixelRect;
        Rect safeArea = Screen.safeArea;

        // Apply uniform padding
        float padding = uniformPaddingPixels;
        safeArea = new Rect(
            safeArea.x + padding,
            safeArea.y + padding,
            Mathf.Max(0, safeArea.width - 2 * padding),
            Mathf.Max(0, safeArea.height - 2 * padding)
        );

        // Clip safe area to canvas rect
        safeArea = Rect.MinMaxRect(
            Mathf.Max(safeArea.xMin, canvasRect.xMin),
            Mathf.Max(safeArea.yMin, canvasRect.yMin),
            Mathf.Min(safeArea.xMax, canvasRect.xMax),
            Mathf.Min(safeArea.yMax, canvasRect.yMax)
        );

        // Debug.Log($"Safe Area: {safeArea}, Canvas Rect: {canvasRect}");

        Vector2 anchorMin = new Vector2(
            (safeArea.xMin - canvasRect.xMin) / canvasRect.width,
            (safeArea.yMin - canvasRect.yMin) / canvasRect.height
        );

        Vector2 anchorMax = new Vector2(
            (safeArea.xMax - canvasRect.xMin) / canvasRect.width,
            (safeArea.yMax - canvasRect.yMin) / canvasRect.height
        );

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}