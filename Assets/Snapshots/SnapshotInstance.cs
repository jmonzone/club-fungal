using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitSnapshot
{
    public string id;
    public Vector3 position;
}

[CreateAssetMenu(fileName = "SnapshotInstance", menuName = "Club Fungal/Snapshot Instance")]
public class SnapshotInstance : ScriptableObject
{
    public List<UnitSnapshot> snapshots = new List<UnitSnapshot>();
}