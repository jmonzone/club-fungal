using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Club Fungal/Units/Unit")]
public class Unit : ScriptableObject
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
