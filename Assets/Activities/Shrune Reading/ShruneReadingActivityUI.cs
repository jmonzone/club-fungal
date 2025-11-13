using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class ShruneReadingActivityUI : ActivityUI<ShruneReadingUnit, ShruneReadingActivityController>
{
    [Header("Shrune Reading UI")]
    [SerializeField] private TypewriterEffect typewriterEffect;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject cardContainer;

    protected override void Awake()
    {
        base.Awake();
        Controller.OnStateChanged += Controller_OnStateChanged;
        continueButton.onClick.AddListener(Controller.StartReading);
    }

    private void Controller_OnStateChanged(ShruneReadingState state)
    {
        SetIntroActive(state == ShruneReadingState.SETUP);
        cardContainer.SetActive(state == ShruneReadingState.CARD);
    }

    private void SetIntroActive(bool value)
    {
        if (value)
        {
            StartCoroutine(typewriterEffect.ShowAndType("What is it that you seek?"));
        }
        else
        {
            StartCoroutine(typewriterEffect.Hide());
        }
    }
}
