using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public struct TransformData
{
    public Vector3 position;
    public Quaternion rotation;
}


public class ShruneReadingActivityUI : ActivityUI<ShruneReadingUnit, ShruneReadingActivityController>
{
    [Header("Shrune Reading References")]
    [SerializeField] private TypewriterEffect titleText;
    [SerializeField] private TypewriterEffect headerText;
    [SerializeField] private TypewriterEffect bodyText;
    [SerializeField] private ShruneReadingSpreadOptionListUI spreadOptionListUI;
    [SerializeField] private Button continueButton;

    [SerializeField] private RectTransform cardContainer;
    [SerializeField] private RectTransform spreadSlotContainer;
    [SerializeField] private RectTransform centerCardAnchor;
    [SerializeField] private RectTransform summaryCardAnchor;

    [SerializeField] private OpenAITextGenerator textGenerator;
    [SerializeField] private TarotCardUI tarotCardPrefab;
    [SerializeField] private Transform tarotCardParent;
    [SerializeField] private List<Transform> shruneSlotParent;

    [Header("Card Container Animation")]
    [SerializeField] private Vector3 cardContainerCenterScale = Vector3.one * 1.5f;
    [SerializeField] private float cardContainerAnimDuration = 0.5f;

    private Vector3 originalCardContainerPosition;
    private string currentCardPrompt = "";

    private List<TarotCardUI> tarotCardList;
    private TarotCardUI selectedCard;

    protected override void Awake()
    {
        base.Awake();
        Controller.OnStateChanged += Controller_OnStateChanged;
        continueButton.onClick.AddListener(() =>
        {
            switch (Controller.CurrentState)
            {
                case ShruneReadingState.INTRO:
                    break;
                case ShruneReadingState.REVEAL:
                    StartCoroutine(AnimateCardToSlot(() => Controller.CompleteReading()));
                    break;
            }
        });

        tarotCardList = new List<TarotCardUI>();
        originalCardContainerPosition = spreadSlotContainer.anchoredPosition;
    }


    protected override void OnPlayerExit(ActivityUnit player)
    {
        base.OnPlayerExit(player);
        StopAllCoroutines();

        foreach (var card in tarotCardList)
        {
            Destroy(card.gameObject);
        }

        tarotCardList = new List<TarotCardUI>();
    }

    private void Controller_OnStateChanged(ShruneReadingState state)
    {
        spreadSlotContainer.gameObject.SetActive(state == ShruneReadingState.CARD || state == ShruneReadingState.REVEAL || state == ShruneReadingState.SUMMARY);

        switch (state)
        {
            case ShruneReadingState.INTRO:
                bodyText.gameObject.SetActive(false);
                titleText.ClearText();
                headerText.ClearText();
                textGenerator.ClearConversationHistory();

                spreadSlotContainer.anchoredPosition = originalCardContainerPosition;
                spreadSlotContainer.localScale = Vector3.one;

                var introExamples = new List<string>
                {
                    "what would you like to seek today?",
                    "what is it now that we wish to uncover?",
                    "i feel like i've been here before",
                    "what in the fungal network is going on here?",
                    "so is it the case that you would like to interface with me?",
                };

                string examplesStr = string.Join(", ", introExamples.Select(p => $"\"{p}\""));
                var instruction = $"Generate something like these examples: {examplesStr}.";

                GeneratePrompt(instruction, 1, (text) =>
                {
                    StartCoroutine(headerText.ShowAndType(text, () =>
                    {
                        StartCoroutine(spreadOptionListUI.PopulateOptions(Controller.AvailableSpreads, option =>
                        {
                            StartCoroutine(spreadOptionListUI.Hide(() =>
                            {
                                Controller.StartReading(option);
                            }));
                        }));
                    }));
                });
                break;
            case ShruneReadingState.CARD:
                continueButton.gameObject.SetActive(false);
                bodyText.gameObject.SetActive(false);
                titleText.ClearText();
                headerText.ClearText();

                StartCoroutine(titleText.ShowAndType(Controller.CurrentSelectionSlot.Label, () =>
                {
                    string examplesStr = string.Join(", ", Controller.CurrentSelectionSlot.Examples.Select(p => $"\"{p}\""));
                    var instruction = $"Generate a prompt similar to these examples: {examplesStr}.";

                    GeneratePrompt(instruction, 1, (text) =>
                    {
                        currentCardPrompt = text;
                        StartCoroutine(headerText.ShowAndType(text, () =>
                        {
                            StartCoroutine(AnimateCardEntry());
                        }));
                    });
                }));
                break;
            case ShruneReadingState.REVEAL:
                bodyText.ClearText();
                selectedCard.StartFlipCard(() =>
                {
                    GenerateInterpretation(selectedCard.Glyph);
                });
                break;
            case ShruneReadingState.SUMMARY:
                continueButton.gameObject.SetActive(false);
                titleText.ClearText();
                headerText.ClearText();
                bodyText.ClearText();

                StartCoroutine(AnimateCardContainerToCenter());

                StartCoroutine(titleText.ShowAndType("The End", () =>
                {
                    GenerateSummaryText();
                }));

                for (int i = 0; i < Controller.RevealedGlyphs.Count && i < tarotCardList.Count; i++)
                {
                    tarotCardList[i].SetGlyph(Controller.RevealedGlyphs[i]);
                    tarotCardList[i].PrepareForSummary();
                }
                break;
            case ShruneReadingState.NULL:
                break;
        }
    }

    // Animation coroutine for card entry
    private IEnumerator AnimateCardEntry()
    {
        var shrune = Instantiate(tarotCardPrefab, cardContainer);
        GlyphData glyph = Controller.SelectedGlyphs[0];
        shrune.PrepareForSelection(glyph, new TransformData { position = new Vector3(0, 800, 0), rotation = Quaternion.identity }, Vector3.one);
        tarotCardList.Add(shrune);

        shrune.OnCardSelected += () =>
        {
            selectedCard = shrune;
            Controller.RevealCard(shrune.Glyph);
        };

        var rectTransform = shrune.GetComponent<RectTransform>();
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 endPos = centerCardAnchor.anchoredPosition;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * 2f;
        float duration = 0.5f;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
            shrune.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
    }

    private IEnumerator AnimateCardToSlot(UnityAction onComplete = null)
    {
        float duration = 0.5f;
        float time = 0;
        Vector3 startPos = selectedCard.transform.position;

        var targetSlot = shruneSlotParent[Controller.RevealedGlyphs.Count - 1];
        Vector3 targetPos = targetSlot.position;
        Vector3 startScale = selectedCard.transform.localScale;
        Vector3 targetScale = Vector3.one;
        Quaternion startRot = selectedCard.transform.localRotation;
        Quaternion targetRot = Quaternion.identity;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            selectedCard.transform.position = Vector3.Lerp(startPos, targetPos, t);
            selectedCard.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            selectedCard.transform.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        selectedCard.transform.SetParent(targetSlot, true);

        onComplete?.Invoke();
    }

    private IEnumerator AnimateCardContainerToCenter()
    {
        Vector3 startPos = spreadSlotContainer.anchoredPosition;
        Vector3 endPos = summaryCardAnchor.anchoredPosition;
        Vector3 startScale = spreadSlotContainer.localScale;
        Vector3 endScale = cardContainerCenterScale;
        float duration = cardContainerAnimDuration;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            spreadSlotContainer.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
            spreadSlotContainer.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        spreadSlotContainer.anchoredPosition = endPos;
        spreadSlotContainer.localScale = endScale;
    }


    private void GenerateText(string prompt, System.Action<string> onGenerated)
    {
        StopAllCoroutines();
        StartCoroutine(textGenerator.GenerateText(prompt, text =>
        {
            var formatRemoveQuotes = text.Replace("\"", "").Trim();
            // onGenerated?.Invoke($"{prompt}:\n{formatRemoveQuotes}");
            Debug.Log($"Generated Text for Prompt:\n{prompt}\nResponse:\n{formatRemoveQuotes}");
            onGenerated?.Invoke($"{formatRemoveQuotes}");
        }));
    }

    private void GeneratePrompt(string instruction, int lines, System.Action<string> onGenerated)
    {
        string prompt = $"{instruction} Respond in exactly {lines} line{(lines > 1 ? "s" : "")}";
        GenerateText(prompt, onGenerated);
    }

    private void GenerateInterpretation(GlyphData glyph)
    {
        var elements = new Dictionary<string, (string desc, float val)>
        {
            { "Fire", ("spirituality, passion, and ability", glyph.Fire) },
            { "Water", ("relationships, emotions, and intuition", glyph.Water) },
            { "Air", ("thinking, freedom, and clarity", glyph.Air) },
            { "Earth", ("the physical world, finances, and stability", glyph.Earth) }
        };

        var maxElement = elements.OrderByDescending(e => e.Value.val).First();

        string position = Controller.CurrentSelectionSlot.Label;

        var mentionShrune = true;

        var instruction = "";

        if (mentionShrune)
        {
            instruction = $"The user was given the prompt: {currentCardPrompt}\n They just selected a Shrune, generate a concise intepretation of This {maxElement.Key} Shrune that is about {glyph.Description}, {maxElement.Value.desc} and connect it to the prompt.";
        }
        else
        {
            instruction = $"The user was given the prompt: {currentCardPrompt}\n Generate a concise insight that is about {glyph.Description}, {maxElement.Value} and how it connects to the prompt.";
        }

        Debug.Log($"Generating interpretation with instruction:\n{instruction} mentionShrune: {mentionShrune}");

        GeneratePrompt(instruction, 1, (text) =>
        {
            bodyText.gameObject.SetActive(true);
            StartCoroutine(bodyText.ShowAndType(text, () => continueButton.gameObject.SetActive(true)));
        });
    }

    private void GenerateSummaryText()
    {
        var examples = new List<string> { "that was the reading, any questions?", "woah what a read, what did I even say?", "well enough of me yapping.", "goodbye spirits, have a good night" };

        string examplesStr = string.Join(", ", examples.Select(p => $"\"{p}\""));
        var instruction = $"Generate something similar to these examples: {examplesStr}.";

        GeneratePrompt(instruction, 1, (text) =>
        {
            StartCoroutine(headerText.ShowAndType(text, () =>
            {
                string instruction = $"Generate a summary interpreting what the Shrunes have divined in the reading from this conversation.";
                GeneratePrompt(instruction, 2, (text) =>
                {
                    StartCoroutine(bodyText.ShowAndType(text));
                });
            }));
        });
    }
}
