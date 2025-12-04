using UnityEngine;

public interface IBuilding 
{
    Vector2Int GridPosition { get; }

    // Размер здания в ячейках (1x1, 2x2, 3x3 и т.д.)
    Vector2Int Size { get; }
}