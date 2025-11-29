using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSpecies", menuName = "Club Fungal/Units/UnitSpecies")]
public class UnitSpecies : ScriptableObject
{
    [SerializeField] private new string name;
    [SerializeField] private Sprite sprite;
    [SerializeField] private GameObject prefab;

    [Tooltip("Column mapping for this UnitSpecies. -1 = keep original, 0–7 = palette index.")]
    [SerializeField] private int[] columnMapping = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };

    [SerializeField] private List<DanceMove> moves;

    public string Name => name;
    public Sprite Sprite => sprite;
    public GameObject Prefab => prefab;
    public List<DanceMove> Moves => moves;

    public int[] ColumnMapping => columnMapping;
}
