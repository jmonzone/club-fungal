using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a unit type defining visual and animation properties.
/// Contains the 2D sprite, 3D game object prefab, and available dance moves (animations).
/// </summary>
[CreateAssetMenu(fileName = "New Unit Species", menuName = "Club Fungal/Units/Unit Species")]
public class UnitSpecies : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private Sprite sprite;
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<DanceMove> moves;

    public string Id => id;
    public Sprite Sprite => sprite;
    public GameObject Prefab => prefab;
    public List<DanceMove> Moves => moves;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = name.ToLower();
        }
    }
}
