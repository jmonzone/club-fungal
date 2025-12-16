using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Unit Template", menuName = "Club Fungal/Units/Unit Template")]
public class UnitTemplate : ScriptableObject
{
    [SerializeField] private UnitData data;
    [SerializeField] private List<UnitMoment> moments;

    public UnitData Data => data;
    public List<UnitMoment> Moments => moments;
}
