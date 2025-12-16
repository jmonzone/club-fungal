using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Job : GURUObject
{
    [SerializeField] private string actionName;
    [SerializeField] private Color actionColor;
    [SerializeField] private Sprite actionSprite;
    [SerializeField] private ActivityReference activity;
    public string ActionName => actionName;
    public Color ActionColor => actionColor;
    public Sprite ActionSprite => actionSprite;
    public ActivityReference Activity => activity;
}
