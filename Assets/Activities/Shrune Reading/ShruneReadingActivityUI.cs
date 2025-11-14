using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShruneReadingActivityUI : ActivityUI<ShruneReadingUnit, ShruneReadingActivityController>
{
    [Header("Shrune Reading References")]
    [SerializeField] private TypewriterEffect typewriterEffect;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject cardContainer;

    private List<TarotCardUI> tarotCardList;

    protected override void Awake()
    {
        base.Awake();
        Controller.OnStateChanged += Controller_OnStateChanged;
        continueButton.onClick.AddListener(Controller.StartReading);

        tarotCardList = GetComponentsInChildren<TarotCardUI>().ToList();
        foreach(var tarotCard in tarotCardList)
        {
            tarotCard.OnCardFlipped += () => Controller.RevealCard();
        }
    }

    private void Controller_OnStateChanged(ShruneReadingState state)
    {
        cardContainer.SetActive(state == ShruneReadingState.CARD);

        switch (state)
        {
            case ShruneReadingState.SETUP:
                StartCoroutine(typewriterEffect.ShowAndType("What is it that you seek?"));
                continueButton.gameObject.SetActive(true);
                break;
            case ShruneReadingState.CARD:
                continueButton.gameObject.SetActive(false);
                break;
            case ShruneReadingState.REVEAL:
                continueButton.gameObject.SetActive(true);
                break;
            case ShruneReadingState.NULL:
                break;
        }
    }
}
