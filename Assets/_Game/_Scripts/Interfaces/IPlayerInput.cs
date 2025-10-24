
using UnityEngine;

public interface IPlayerInput
{
    /// <summary>
    /// Возвращает направление движения как вектор (x,y), нормализованный или ноль
    /// </summary>
    /// <returns></returns>
    Vector2 GetMovement();
}
