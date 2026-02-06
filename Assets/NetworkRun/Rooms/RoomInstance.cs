using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomInstance
{
    [SerializeField] private RoomTemplate template;
    [SerializeField] private RoomData data;

    public RoomData Data => data;

    public RoomInstance(RoomData data)
    {
        this.data = data;
    }

    public RoomInstance(RoomTemplate template)
    {
        this.template = template;
        data = new RoomData
        {
            id = template.Data.id,
            name = template.name,
        };
    }
}
