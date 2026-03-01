using System.Collections.Generic;
using UnityEngine;

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

    [Header("Species Components")]
    [SerializeField] private List<UnitComponentDefinition> components;

    public UnitType Type => type;
    public Sprite Sprite => sprite;
    public GameObject Prefab => prefab;
    public List<DanceMove> Moves => moves;
    public List<UnitComponentDefinition> Components => components;
}
