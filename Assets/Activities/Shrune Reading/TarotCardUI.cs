using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TarotCardUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Transform front;
    [SerializeField] private Transform back;
    [SerializeField] private Image glyphImage;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool isFaceDown = true;

    public event UnityAction<TarotCardUI> OnCardFlipped;
    public event UnityAction OnFlipStarted;
    public event UnityAction OnCardSelected;

    public GlyphData Glyph { get; private set; }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            Debug.Log($"Card {name} selected.");
            OnCardSelected?.Invoke();
        });
    }

    public void Reset()
    {
        FlipCard(true);
    }

    public void SetGlyph(GlyphData glyph)
    {
        Glyph = glyph;
        glyphImage.sprite = glyph.Sprite;
    }

    public void StartFlipCard(UnityAction onComplete = null)
    {
        OnFlipStarted?.Invoke();
        StartCoroutine(FlipCardRoutine(onComplete));
    }

    private IEnumerator FlipCardRoutine(UnityAction onComplete)
    {
        var i = 0f;
        while (i < 90f)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            i += rotationSpeed * Time.deltaTime;

            yield return null;
        }

        FlipCard(!isFaceDown);

        while (i < 180f)
        {
            transform.Rotate(Vector3.down, rotationSpeed * Time.deltaTime);
            i += rotationSpeed * Time.deltaTime;

            yield return null;
        }

        onComplete?.Invoke();
        OnCardFlipped?.Invoke(this);
    }

    private void FlipCard(bool isFaceDown)
    {
        this.isFaceDown = isFaceDown;

        front.gameObject.SetActive(!this.isFaceDown);
        back.gameObject.SetActive(this.isFaceDown);
    }

    public void Reveal()
    {
        FlipCard(false);
    }

    public void PrepareForSelection(GlyphData glyph, TransformData transformData, Vector3 scale)
    {
        if (glyph != null)
        {
            SetGlyph(glyph);
        }
        Reset();
        gameObject.SetActive(true);
        transform.localPosition = transformData.position;
        transform.localRotation = transformData.rotation;
        transform.localScale = scale;
        SetInteractable(true);
    }

    public void PrepareForReveal()
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }

    public void PrepareForSummary()
    {
        gameObject.SetActive(true);
        Reveal();
    }
}
