using UnityEngine;

/// <summary>
/// Asset reference for units using Quirky Series package models.
/// Extends UnitSpecies with color palette mapping to adjust the visual appearance of each column.
/// </summary>
[CreateAssetMenu(fileName = "New Quirky Series Unit Species", menuName = "Club Fungal/Units/Quirky Series Unit Species")]
public class QuirkySeriesUnitSpecies : UnitSpecies
{
    [Tooltip("Column mapping for this Unit. -1 = keep original, 0–7 = palette index.")]
    [SerializeField] private int[] columnMapping = new int[8] { -1, -1, -1, -1, -1, -1, -1, -1 };

    public int[] ColumnMapping => columnMapping;
}