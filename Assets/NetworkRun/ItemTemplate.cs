using UnityEngine;

[CreateAssetMenu(fileName = "ItemTemplate", menuName = "Club Fungal/Network Run/Item Template")]
public class ItemTemplate : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite sprite;
    [SerializeField] private string description;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Sprite => sprite;
    public string Description => description;
}
