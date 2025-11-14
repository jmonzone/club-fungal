using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShruneReadingActivityUI : ActivityUI<ShruneReadingUnit, ShruneReadingActivityController>
{
    [Header("Shrune Reading References")]
    [SerializeField] private TypewriterEffect promptText;
    [SerializeField] private TypewriterEffect interpretationText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject cardContainer;

    private List<TarotCardUI> tarotCardList;
    private TarotCardUI selectedCard;
    private Dictionary<TarotCardUI, Vector3> originalPositions = new();

    protected override void Awake()
    {
        base.Awake();
        Controller.OnStateChanged += Controller_OnStateChanged;
        continueButton.onClick.AddListener(() =>
        {
            switch (Controller.CurrentState)
            {
                case ShruneReadingState.SETUP:
                    Controller.StartReading();
                    break;
                case ShruneReadingState.REVEAL:
                    Controller.CompleteReading();
                    break;
            }
        });

        tarotCardList = GetComponentsInChildren<TarotCardUI>().ToList();
        foreach (var tarotCard in tarotCardList)
        {
            originalPositions[tarotCard] = tarotCard.transform.localPosition;
            tarotCard.OnCardFlipped += (card) =>
            {
                selectedCard = card;
                Controller.RevealCard(card);
            };
        }
    }

    private void Controller_OnStateChanged(ShruneReadingState state)
    {
        cardContainer.SetActive(state == ShruneReadingState.CARD || state == ShruneReadingState.REVEAL || state == ShruneReadingState.SUMMARY);

        switch (state)
        {
            case ShruneReadingState.SETUP:
                StartCoroutine(promptText.ShowAndType("What is it that you seek?"));
                continueButton.gameObject.SetActive(true);
                interpretationText.gameObject.SetActive(false);
                break;
            case ShruneReadingState.CARD:
                continueButton.gameObject.SetActive(false);
                interpretationText.gameObject.SetActive(false);
                for (int i = 0; i < tarotCardList.Count; i++)
                {
                    var card = tarotCardList[i];
                    if (i < Controller.SelectedGlyphs.Count)
                    {
                        card.SetGlyph(Controller.SelectedGlyphs[i]);
                    }
                    card.Reset();
                    card.gameObject.SetActive(true);
                    card.transform.localPosition = originalPositions[card];
                    card.transform.localScale = Vector3.one;
                }
                break;
            case ShruneReadingState.REVEAL:
                StartCoroutine(interpretationText.ShowAndType("Spores spores and spores"));
                continueButton.gameObject.SetActive(true);
                foreach (var card in tarotCardList)
                {
                    if (card != selectedCard)
                    {
                        card.gameObject.SetActive(false);
                    }
                    else
                    {
                        card.transform.localPosition = Vector3.zero;
                        card.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    }
                }
                break;
            case ShruneReadingState.SUMMARY:
                continueButton.gameObject.SetActive(false);
                interpretationText.gameObject.SetActive(true);
                StartCoroutine(interpretationText.ShowAndType("The reading is complete."));
                foreach (var card in Controller.RevealedCards)
                {
                    card.gameObject.SetActive(true);
                    card.transform.localPosition = originalPositions[card];
                    card.transform.localScale = Vector3.one;
                    card.Reveal();
                }
                foreach (var card in tarotCardList.Where(c => !Controller.RevealedCards.Contains(c)))
                {
                    card.gameObject.SetActive(false);
                }
                break;
            case ShruneReadingState.NULL:
                break;
        }
    }
}
