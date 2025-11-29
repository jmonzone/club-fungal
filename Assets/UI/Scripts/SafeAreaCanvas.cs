using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Canvas canvas;

    private void OnValidate()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        ApplySafeArea();
    }

    public void ApplySafeArea()
    {
        Rect canvasRect = canvas.pixelRect;
        Rect safeArea = Screen.safeArea;

        // Clip safe area to canvas rect
        safeArea = Rect.MinMaxRect(
            Mathf.Max(safeArea.xMin, canvasRect.xMin),
            Mathf.Max(safeArea.yMin, canvasRect.yMin),
            Mathf.Min(safeArea.xMax, canvasRect.xMax),
            Mathf.Min(safeArea.yMax, canvasRect.yMax)
        );

        Debug.Log($"Safe Area: {safeArea}, Canvas Rect: {canvasRect}");

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