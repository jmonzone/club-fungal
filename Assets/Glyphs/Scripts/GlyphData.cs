using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GlyphData : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private int fire;
    [SerializeField] private int water;
    [SerializeField] private int air;
    [SerializeField] private int earth;

    private static readonly Dictionary<(int fire, int water, int air, int earth), string> descriptions = new Dictionary<(int, int, int, int), string>
    {
        {(1, 0, 0, 0), "a rush, an epiphany, sudden energy"},
        {(2, 0, 0, 0), "making a plan, visualizing, optimism"},
        {(3, 0, 0, 0), "taking first steps, courage, readiness"},
        {(4, 0, 0, 0), "a celebration, success, a checkpoint"},
        {(0, 1, 0, 0), "a feeling, an instict, a connection"},
        {(0, 2, 0, 0), "partnership, being in agreement, acknowleding a feeling"},
        {(0, 3, 0, 0), "being a part of something, community, feeling seen"},
        {(0, 4, 0, 0), "having doubt, uncertainty on what is for you"},
        {(0, 0, 1, 0), "new ideas, a memory, a perspective shift"},
        {(0, 0, 2, 0), "making descisions, taking a risk, trusting yourself"},
        {(0, 0, 3, 0), "pain, regret, tragedy"},
        {(0, 0, 4, 0), "recovering from the past, learning from mistakes, resting"},
        {(0, 0, 0, 1), "an opportunity, a foot in the door, an investment"},
        {(0, 0, 0, 2), "being balanced, juggling between two things, understanding costs"},
        {(0, 0, 0, 3), "teamwork, collaboration, shared goals, assigned roles"},
        {(0, 0, 0, 4), "taking things into your own hands, greed, doubt" },
    };

    public string Description
    {
        get
        {
            if (descriptions.TryGetValue((fire, water, air, earth), out var desc))
                return desc;
            return "none";
        }
    }

    [SerializeField] private Element element;
    [SerializeField] private int tier;

    private Color earthColor = new Color32(0x8A, 0xCE, 0x00, 0xFF); // #8ACE00
    private Color fireColor = new Color32(0xF1, 0x7B, 0x7B, 0xFF); // #F17B7B
    private Color waterColor = new Color32(0x5B, 0xD2, 0xEC, 0xFF); // #5BD2EC
    private Color airColor = new Color32(0xF9, 0xC8, 0x1A, 0xFF); // #F9C81A

    public string Id => sprite.name;
    public int Fire => fire;
    public int Water => water;
    public int Air => air;
    public int Earth => earth;

    public int Tier => tier;
    public Element Element => element;
    public bool IsPure => (Fire > 0 ? 1 : 0) + (Water > 0 ? 1 : 0) + (Air > 0 ? 1 : 0) + (Earth > 0 ? 1 : 0) == 1;

    public Color Color
    {
        get
        {
            Color blendedColor =
                fireColor * (fire / tier) +
                waterColor * (water / tier) +
                earthColor * (earth / tier) +
                airColor * (air / tier);
            return blendedColor;
        }
    }
    public Sprite Sprite => sprite;

    public void Initialize()
    {
        // Reset values
        fire = water = air = earth = 0;
        element = Element.NONE;
        tier = 0;

        // Parse counts from name (e.g. "3a2f1w")
        var matches = System.Text.RegularExpressions.Regex.Matches(name, @"(\d+)([a-zA-Z])");
        foreach (System.Text.RegularExpressions.Match m in matches)
        {
            int value = int.Parse(m.Groups[1].Value);
            char symbol = char.ToLower(m.Groups[2].Value[0]);

            switch (symbol)
            {
                case 'f': fire = value; break;
                case 'w': water = value; break;
                case 'a': air = value; break;
                case 'e': earth = value; break;
            }
        }

        // Compute tier as total number of components
        tier = fire + water + air + earth;

        // Compute weighted element

        var weightThreshold = .4;

        int total = tier;
        if (total > 0)
        {
            // Weight each element proportional to its count
            float fWeight = fire / (float)total;
            float wWeight = water / (float)total;
            float aWeight = air / (float)total;
            float eWeight = earth / (float)total;

            element = Element.NONE;

            // If weight > 0, include the element
            if (fWeight > weightThreshold) element |= Element.FIRE;
            if (wWeight > weightThreshold) element |= Element.WATER;
            if (aWeight > weightThreshold) element |= Element.AIR;
            if (eWeight > weightThreshold) element |= Element.EARTH;
        }
    }
}

[Flags]
public enum Element
{
    NONE = 0,
    FIRE = 1 << 0,
    WATER = 1 << 1,
    EARTH = 1 << 2,
    AIR = 1 << 3
}