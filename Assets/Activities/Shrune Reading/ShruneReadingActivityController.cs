using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

public enum ShruneReadingState
{
    NULL,
    INTRO,
    CARD,
    REVEAL,
    SUMMARY,
}

public class ShruneReadingActivityController : ActivityController<ShruneReadingUnit>
{
    [Header("Shrune Reading References")]
    [SerializeField] private GlyphCollection glyphCollection;
    [SerializeField] private List<ShruneReadingSpread> availableSpreads = new();
    [SerializeField] private Sprite mindBodySpiritIcon;
    [SerializeField] private Sprite situationProblemAdviceIcon;
    [SerializeField] private Sprite pastPresentFutureIcon;

    [Header("Shrune Reading Runtime Data")]
    [SerializeField] private ShruneReadingState currentState;
    [SerializeField] private int cardIndex = 0;
    [SerializeField] private List<GlyphData> selectedGlyphs = new();
    [SerializeField] private List<GlyphData> revealedGlyphs = new();

    private ShruneReadingSpread selectedSpread;

    public List<ShruneReadingSpread> AvailableSpreads => availableSpreads;

    public ShruneReadingState CurrentState => currentState;
    public List<GlyphData> SelectedGlyphs => selectedGlyphs;
    public List<GlyphData> RevealedGlyphs => revealedGlyphs;
    public ShruneReadingSpread SelectedSpread => selectedSpread;
    public ShruneReadingSpread.SpreadSlot CurrentSelectionSlot => selectedSpread.SpreadSlots[cardIndex];
    public event UnityAction<ShruneReadingState> OnStateChanged;
    protected override void Awake()
    {
        base.Awake();
        availableSpreads = new List<ShruneReadingSpread>
        {
            ShruneReadingSpread.CreateMindBodySpiritSpread(mindBodySpiritIcon),
            ShruneReadingSpread.CreateSituationProblemAdviceSpread(situationProblemAdviceIcon),
            ShruneReadingSpread.CreatePastPresentFutureSpread( pastPresentFutureIcon),
        };
    }

    private void SetState(ShruneReadingState state)
    {
        currentState = state;
        OnStateChanged?.Invoke(state);
    }

    protected override void OnPlayerEnter(ActivityUnit player)
    {
        base.OnPlayerEnter(player);
        cardIndex = -1;
        revealedGlyphs.Clear();
        SetState(ShruneReadingState.INTRO);
    }

    public void StartReading(ShruneReadingSpread spread)
    {
        selectedSpread = spread;
        ShowNextCard();
    }

    private void ShowNextCard()
    {
        var availableGlyphs = glyphCollection.GetPureGlyphs().Except(revealedGlyphs).ToList();
        selectedGlyphs = availableGlyphs.OrderBy(x => Random.value).Take(3).ToList();
        cardIndex++;
        SetState(ShruneReadingState.CARD);
    }

    public void RevealCard(GlyphData glyph)
    {
        revealedGlyphs.Add(glyph);
        SetState(ShruneReadingState.REVEAL);
    }

    public void CompleteReading()
    {
        if ((cardIndex + 1) >= selectedSpread.SpreadSlots.Count)
        {
            SetState(ShruneReadingState.SUMMARY);
        }
        else
        {
            ShowNextCard();
        }
    }

    protected override void OnPlayerExit(ActivityUnit player)
    {
        base.OnPlayerExit(player);
        SetState(ShruneReadingState.NULL);
    }

}
