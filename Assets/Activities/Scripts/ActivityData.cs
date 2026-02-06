using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActivityData
{
    public string id;
    public string name;
    public List<UnitInstance> units = new List<UnitInstance>();
}
