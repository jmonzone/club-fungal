using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private FadeCanvasGroup fadeCanvasGroup;
    [SerializeField] private TextMeshProUGUI textUI;
    [SerializeField] private float baseSpeed = 0.03f;
    [SerializeField] private float punctuationPause = 0.2f;

    public IEnumerator ShowAndType(string fullText, UnityAction onComplete = null)
    {
        yield return fadeCanvasGroup.FadeIn();
        yield return TypeRoutine(fullText, onComplete);
    }

    public IEnumerator Hide()
    {
        yield return fadeCanvasGroup.FadeOut();
    }

    public IEnumerator TypeRoutine(string fullText, UnityAction onComplete = null)
    {
        textUI.text = "";

        foreach (char c in fullText)
        {
            textUI.text += c;

            float delay = baseSpeed;
            if (".,!?:;".Contains(c.ToString()))
                delay += punctuationPause;

            delay *= Random.Range(0.9f, 1.3f);
            yield return new WaitForSeconds(delay);
        }

        onComplete?.Invoke();
    }

    public void ClearText()
    {
        textUI.text = "";
    }
}
