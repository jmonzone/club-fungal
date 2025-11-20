using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ShruneReadingSpread", menuName = "Club Fungal/Activities/Shrune Reading/Shrune Reading Spread")]
public class ShruneReadingSpread : ScriptableObject
{
    [System.Serializable]
    public struct SpreadSlot
    {
        public string Label;
        public List<string> Examples;
    }

    [SerializeField] private string spreadName;
    [SerializeField] private Sprite icon;

    [SerializeField] private List<SpreadSlot> spreadSlots = new();
    [SerializeField] private List<string> introExamples = new();

    public string SpreadName => spreadName;
    public Sprite Icon => icon;
    public List<SpreadSlot> SpreadSlots => spreadSlots;
    public List<string> IntroExamples => introExamples;

    public static ShruneReadingSpread CreatePastPresentFutureSpread(Sprite sprite)
    {
        var spread = CreateInstance<ShruneReadingSpread>();
        spread.spreadName = "Past, Present, Future";
        spread.icon = sprite;
        spread.introExamples = new List<string> { "what is it that we seek?", "what do we wish to uncover", "time is an illusion, have we been here before?", "are we ready for the ride!" };
        spread.spreadSlots = new List<SpreadSlot>
        {
            new SpreadSlot { Label = "Past", Examples = new List<string>
            {
                "you have recently been troubled, haven't you?",
                "it seems like something from your past is coming up",
                // "is it ok if we look at the past first?",
                "there was something that happened to you in the past",
                // "i see a memory surfacing from long ago",
                // "let's start at the beggining.",
                // "there is a story to be told",
                "are you running away from something?",
            } },
            new SpreadSlot { Label = "Present", Examples = new List<string>
            {
                "and where are we in the now, the current?",
                "let's look at what is right in front of us",
                "what is it that has your attention?",
                "what have you noticed lately?"
            } },
            new SpreadSlot { Label = "Future", Examples = new List<string> { "is it true that the future is not set?", "do I believe that the near future is tangible?", "who knows how far ahead we can see?", "what can we see in the days ahead of us?" } }
        };
        return spread;
    }

    public static ShruneReadingSpread CreateMindBodySpiritSpread(Sprite sprite)
    {
        var spread = CreateInstance<ShruneReadingSpread>();
        spread.spreadName = "Mind, Body, Spirit";
        spread.icon = sprite;
        spread.introExamples = new List<string> { "let us explore the self", "what lies within", "the journey inward", "discover your essence" };
        spread.spreadSlots = new List<SpreadSlot>
        {
            new SpreadSlot { Label = "Mind", Examples = new List<string> { "what are the thoughts that occupy you?", "are we in touch with our mental state", "what do we have on our mind", "where does our intellect and pursuits lie?" } },
            new SpreadSlot { Label = "Body", Examples = new List<string> { "how is the vessel we inhabit?", "how is our physical well-being?", "how do we feel in our skin?", "how is the strength and health of your form?" } },
            new SpreadSlot { Label = "Spirit", Examples = new List<string> { "where is the essence of our being", "where is our inner light?", "what is driving our soul", "do we have a connection to the greater whole?" } }
        };
        return spread;
    }

    public static ShruneReadingSpread CreateSituationProblemAdviceSpread(Sprite sprite)
    {
        var spread = CreateInstance<ShruneReadingSpread>();
        spread.spreadName = "Situation, Problem, Advice";
        spread.icon = sprite;
        spread.introExamples = new List<string> { "let us delve into the situation?", "what challenges await us?", "we are here to seek guidance", "let's look into the path ahead" };
        spread.spreadSlots = new List<SpreadSlot>
        {
            new SpreadSlot { Label = "Situation", Examples = new List<string> { "what are the current state of affairs?", "what is happening right now?", "what is the context of our life?", "let's check into the environment surrounding us?" } },
            new SpreadSlot { Label = "Problem", Examples = new List<string> { "what are the obstacles in our way?", "what is hindering me?", "what are the challenges we must overcome?", "what are the issues at hand?" } },
            new SpreadSlot { Label = "Advice", Examples = new List<string> { "what will guide us on our journey?", "what steps do we need to take next", "what wisdom do we need to move forward?", "how can we navigate the path ahead?" } }
        };
        return spread;
    }
}