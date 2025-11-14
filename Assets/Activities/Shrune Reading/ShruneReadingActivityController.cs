using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

public enum ShruneReadingState
{
    NULL,
    SETUP,
    CARD,
    REVEAL,
    SUMMARY,
}

public class ShruneReadingActivityController : ActivityController<ShruneReadingUnit>
{
    [Header("Shrune Reading References")]
    [SerializeField] private GlyphCollection glyphCollection;

    [Header("Shrune Reading Runtime Data")]
    [SerializeField] private ShruneReadingState currentState;
    [SerializeField] private int cardsRead = 0;
    [SerializeField] private List<GlyphData> selectedGlyphs = new();
    [SerializeField] private List<TarotCardUI> revealedCards = new();

    public ShruneReadingState CurrentState => currentState;
    public List<GlyphData> SelectedGlyphs => selectedGlyphs;
    public List<TarotCardUI> RevealedCards => revealedCards;

    public event UnityAction<ShruneReadingState> OnStateChanged;

    private void SetState(ShruneReadingState state)
    {
        currentState = state;
        OnStateChanged?.Invoke(state);
    }

    protected override void OnPlayerEnter(ActivityUnit player)
    {
        base.OnPlayerEnter(player);
        cardsRead = 0;
        revealedCards.Clear();
        SetState(ShruneReadingState.SETUP);
    }

    public void StartReading()
    {
        selectedGlyphs = glyphCollection.GetPureGlyphs().OrderBy(x => Random.value).Take(3).ToList();
        SetState(ShruneReadingState.CARD);
    }

    public void RevealCard(TarotCardUI card)
    {
        revealedCards.Add(card);
        cardsRead++;
        SetState(ShruneReadingState.REVEAL);
    }

    public void CompleteReading()
    {
        if (cardsRead >= 3)
        {
            SetState(ShruneReadingState.SUMMARY);
        }
        else
        {
            SetState(ShruneReadingState.CARD);
        }
    }

    protected override void OnPlayerExit(ActivityUnit player)
    {
        base.OnPlayerExit(player);
        SetState(ShruneReadingState.NULL);
    }

}
