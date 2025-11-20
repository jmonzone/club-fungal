using UnityEngine;

[CreateAssetMenu(fileName = "StoryData", menuName = "Club Fungal/Story/Story Data")]
public class StoryData : ScriptableObject
{
    [SerializeField] private string id;
    public string Id => id;
}
