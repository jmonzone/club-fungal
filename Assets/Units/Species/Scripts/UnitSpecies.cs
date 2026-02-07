using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    None,
    Aqua,
    Sky,
    Paw
}

/// <summary>
/// Represents a unit type defining visual and animation properties.
/// Contains the 2D sprite, 3D game object prefab, and available dance moves (animations).
/// </summary>
[CreateAssetMenu(fileName = "New Unit Species", menuName = "Club Fungal/Units/Unit Species")]
public class UnitSpecies : GURUObject
{
    [SerializeField] private UnitType type;
    [SerializeField] private Sprite sprite;
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<DanceMove> moves;

    public UnitType Type => type;
    public Sprite Sprite => sprite;
    public GameObject Prefab => prefab;
    public List<DanceMove> Moves => moves;
}
