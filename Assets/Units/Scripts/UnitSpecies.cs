using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Species", menuName = "Club Fungal/Units/Unit Species")]
public class UnitSpecies : ScriptableObject
{
    [SerializeField] private new string name;
    [SerializeField] private Sprite sprite;
    [SerializeField] private GameObject prefab;

    [SerializeField] private List<DanceMove> moves;

    public string Name => name;
    public Sprite Sprite => sprite;
    public GameObject Prefab => prefab;
    public List<DanceMove> Moves => moves;
}
