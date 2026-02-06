using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomInstanceService", menuName = "Club Fungal/Rooms/Room Instance Service")]
public class RoomInstanceService : GURUService
{
    [Header("References")]
    [SerializeField] private LocalData localData;
    public LocalData LocalData => localData;

    [Header("Collections")]
    [SerializeField] private List<RoomTemplate> initialRooms;
    [SerializeField] private List<RoomInstance> rooms;

    public List<RoomTemplate> InitialRooms => initialRooms;
    public List<RoomInstance> Instances => rooms;

    private const string ROOM_KEY = "rooms";

    protected override void OnInitialize()
    {
        Debug.Log("Initializing RoomInstanceService");
        rooms = new List<RoomInstance>();

        if (localData.JsonFile.ContainsKey(ROOM_KEY))
        {
            var roomsJson = localData.JsonFile[ROOM_KEY].ToString();
            var roomsData = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RoomData>>(roomsJson);

            foreach (var roomData in roomsData)
            {
                var roomInstance = new RoomInstance(roomData);
                rooms.Add(roomInstance);
            }
        }
        else
        {
            foreach (var template in initialRooms)
            {
                var roomInstance = new RoomInstance(template);
                rooms.Add(roomInstance);
            }
        }

        Save();
    }

    public void Save()
    {
        if (localData == null)
        {
            Debug.LogError("LocalData is not assigned in RoomInstanceService");
            return;
        }
        if (localData.JsonFile == null) localData.Initialize();

        var roomsData = rooms.Select(room => room.Data).ToList();
        var json = JsonConvert.SerializeObject(roomsData, Formatting.Indented);
        localData.SaveData(ROOM_KEY, JToken.Parse(json));
    }
}
