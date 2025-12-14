using UnityEngine;

[CreateAssetMenu(fileName = "FungalUnit", menuName = "Club Fungal/Units/Fungal Unit")]
public class FungalUnit : UnitSpecies
{
    [Tooltip("Column mapping for this Unit. -1 = keep original, 0–7 = palette index.")]
    [SerializeField] private int[] columnMapping = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };

    public int[] ColumnMapping => columnMapping;
}