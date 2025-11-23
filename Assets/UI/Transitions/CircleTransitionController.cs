using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class CircleTransitionController : MonoBehaviour
{
    [SerializeField] private TransitionReference transitionReference;
    [SerializeField] private Image circleImage;
    [SerializeField] private float maxSize = 1f;
    [SerializeField] private float circleInDuration = 1f;
    [SerializeField] private AnimationCurve circleInCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float circleOutDuration = 0.75f;
    [SerializeField] private AnimationCurve circleOutCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private void OnEnable()
    {
        transitionReference.OnTransitionStarted += HandleTransitionStart;
    }

    private void OnDisable()
    {
        transitionReference.OnTransitionStarted -= HandleTransitionStart;
    }

    private void HandleTransitionStart(TransitionType type, UnityAction onComplete = null)
    {
        StopAllCoroutines();

        if (type == TransitionType.CIRCLE_IN)
        {
            StartCoroutine(LerpCircleSize(maxSize, 0f, circleInDuration, circleInCurve, onComplete)); // Circle in
        }
        else if (type == TransitionType.CIRCLE_OUT)
        {
            StartCoroutine(LerpCircleSize(0f, maxSize, circleOutDuration, circleOutCurve, onComplete)); // Circle out
        }
    }

    private IEnumerator LerpCircleSize(float from, float to, float duration, AnimationCurve curve, UnityAction onComplete = null)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = curve.Evaluate(t);
            float size = Mathf.Lerp(from, to, curveValue);
            circleImage.material.SetFloat("_Size", size);
            elapsed += Time.deltaTime;
            yield return null;
        }
        circleImage.material.SetFloat("_Size", to);
        onComplete?.Invoke();
    }
}
