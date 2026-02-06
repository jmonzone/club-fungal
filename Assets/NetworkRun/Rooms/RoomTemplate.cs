using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Room Template", menuName = "Club Fungal/Rooms/Room Template")]
public class RoomTemplate : ScriptableObject
{
    [SerializeField] private RoomData data;

    public RoomData Data => data;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(data.id))
        {
            data.id = GetNextRoomId().ToString("D12");
            EditorUtility.SetDirty(this);
        }

        if (string.IsNullOrEmpty(data.name))
        {
            data.name = this.name;
        }
    }

    private static int GetNextRoomId()
    {
        var allRoomTemplates = AssetDatabase.FindAssets("t:RoomTemplate")
            .Select(guid => AssetDatabase.LoadAssetAtPath<RoomTemplate>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(template => template != null && !string.IsNullOrEmpty(template.Data?.id))
            .ToList();

        if (allRoomTemplates.Count == 0)
            return 0;

        int maxId = allRoomTemplates
            .Max(template => int.TryParse(template.Data.id, out int id) ? id : -1);

        return maxId + 1;
    }
#endif
}
