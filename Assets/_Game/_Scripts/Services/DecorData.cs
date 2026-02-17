using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class DecorData : MonoBehaviour
{
    
    [Inject] private PlayerMovement _playerMovement;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _playerSpriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerSpriteRenderer=_playerMovement.GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        UpdateSortingOrder();
        CheckPlayerOverlap();
    }
    

    private void CheckPlayerOverlap()
    {
        // Получаем позицию игрока в локальных координатах спрайта
        Vector3 localPlayerPos = transform.InverseTransformPoint(_playerMovement.transform.position);

        // Проверяем, есть ли непрозрачный пиксель в позиции игрока
        bool isOverlapping = IsPixelSolid(localPlayerPos);

        // Анимируем прозрачность
        float targetAlpha = isOverlapping ? 0.3f : 1f;

        if (Mathf.Abs(_spriteRenderer.color.a - targetAlpha) > 0.01f)
        {
            _spriteRenderer.DOFade(targetAlpha, 0.25f);
        }
    }

    private bool IsPixelSolid(Vector3 localPosition)
    {
        Sprite sprite = _spriteRenderer.sprite;
        if (sprite == null) return false;

        Texture2D texture = sprite.texture;
        Rect rect = sprite.rect;

        // Конвертируем локальную позицию в пиксельные координаты спрайта
        float pixelsPerUnit = sprite.pixelsPerUnit;
        Vector2 pivot = sprite.pivot;

        // Переводим в координаты текстуры
        int pixelX = Mathf.RoundToInt(pivot.x + localPosition.x * pixelsPerUnit);
        int pixelY = Mathf.RoundToInt(pivot.y + localPosition.y * pixelsPerUnit);

        // Проверяем границы спрайта
        if (pixelX < 0 || pixelX >= rect.width ||
            pixelY < 0 || pixelY >= rect.height)
        {
            return false;
        }

        // Получаем цвет пикселя из текстуры
        // Важно: GetPixel работает с координатами всей текстуры, а не rect спрайта
        int textureX = Mathf.RoundToInt(rect.x + pixelX);
        int textureY = Mathf.RoundToInt(rect.y + pixelY);

        Color pixelColor = texture.GetPixel(textureX, textureY);

        // Возвращаем true если пиксель видимый (альфа больше порога)
        return pixelColor.a > 0.1f;
    }




    private void UpdateSortingOrder()
    {
        // Получаем верхнюю точку на той стороне коллайдера, где находится игрок
        Vector2 topPoint = GetTopPointOnPlayerSide();

        // Получаем боковую точку на стороне игрока (для прозрачности)
        Vector2 sidePoint = GetSidePointOnPlayerSide();

        // Используем Y координату выбранной точки для расчета adjustedY
        float adjustedY = topPoint.y;

        // Используем X координату боковой точки для расчета прозрачности
        float sideX = sidePoint.x;

        // Расчет sorting order
        float deltaY = _playerMovement.transform.position.y - adjustedY;
        int offset = Mathf.RoundToInt(deltaY);
        _spriteRenderer.sortingOrder = _playerSpriteRenderer.sortingOrder + offset;

        
    }
    
   

    
    // Новый вспомогательный метод для получения боковой точки на стороне игрока
    private Vector2 GetSidePointOnPlayerSide()
    {
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider == null)
        {
            // Если нет PolygonCollider2D, используем крайнюю точку спрайта на стороне игрока
            float playerX = _playerMovement.transform.position.x;
            float sideX = playerX > transform.position.x
                ? transform.position.x + _spriteRenderer.bounds.extents.x
                : transform.position.x - _spriteRenderer.bounds.extents.x;

            return new Vector2(sideX, transform.position.y);
        }

        // Находим ближайшую точку на коллайдере к игроку
        Vector2 playerPos = _playerMovement.transform.position;
        Vector2 closestPoint = polygonCollider.ClosestPoint(playerPos);

        // Определяем, с какой стороны от центра находится ближайшая точка
        bool isRightSide = closestPoint.x > transform.position.x;

        // Ищем самую крайнюю точку на этой стороне (максимальный X для правой стороны, минимальный X для левой)
        Vector2 sidePointOnSide = Vector2.zero;
        float targetX = isRightSide ? float.NegativeInfinity : float.PositiveInfinity;

        for (int i = 0; i < polygonCollider.pathCount; i++)
        {
            Vector2[] points = polygonCollider.GetPath(i);

            foreach (Vector2 point in points)
            {
                Vector2 worldPoint = transform.TransformPoint(point);

                // Проверяем, находится ли точка на нужной стороне
                bool pointIsOnRightSide = worldPoint.x > transform.position.x;

                if (pointIsOnRightSide == isRightSide)
                {
                    // Ищем самую крайнюю точку на этой стороне
                    if (isRightSide)
                    {
                        // Для правой стороны ищем максимальный X
                        if (worldPoint.x > targetX)
                        {
                            targetX = worldPoint.x;
                            sidePointOnSide = worldPoint;
                        }
                    }
                    else
                    {
                        // Для левой стороны ищем минимальный X
                        if (worldPoint.x < targetX)
                        {
                            targetX = worldPoint.x;
                            sidePointOnSide = worldPoint;
                        }
                    }
                }
            }
        }

        // Если не нашли точек на нужной стороне, используем ближайшую точку
        if ((isRightSide && float.IsNegativeInfinity(targetX)) ||
            (!isRightSide && float.IsPositiveInfinity(targetX)))
        {
            return closestPoint;
        }

        return sidePointOnSide;
    }

    // Вспомогательный метод для получения верхней точки на стороне игрока (без изменений)
    private Vector2 GetTopPointOnPlayerSide()
    {
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider == null)
        {
            // Если нет PolygonCollider2D, используем верхнюю точку спрайта на стороне игрока
            float playerX = _playerMovement.transform.position.x;
            float sideX = playerX > transform.position.x
                ? transform.position.x + _spriteRenderer.bounds.extents.x
                : transform.position.x - _spriteRenderer.bounds.extents.x;

            return new Vector2(sideX, transform.position.y + _spriteRenderer.bounds.extents.y);
        }

        // Находим ближайшую точку на коллайдере к игроку
        Vector2 playerPos = _playerMovement.transform.position;
        Vector2 closestPoint = polygonCollider.ClosestPoint(playerPos);

        // Определяем, с какой стороны от центра находится ближайшая точка
        bool isRightSide = closestPoint.x > transform.position.x;

        // Ищем самую высокую точку на этой стороне
        Vector2 topPointOnSide = Vector2.negativeInfinity;
        float highestY = float.NegativeInfinity;

        for (int i = 0; i < polygonCollider.pathCount; i++)
        {
            Vector2[] points = polygonCollider.GetPath(i);

            foreach (Vector2 point in points)
            {
                Vector2 worldPoint = transform.TransformPoint(point);

                // Проверяем, находится ли точка на нужной стороне
                bool pointIsOnRightSide = worldPoint.x > transform.position.x;

                if (pointIsOnRightSide == isRightSide)
                {
                    // Ищем самую высокую точку на этой стороне
                    if (worldPoint.y < highestY)
                    {
                        highestY = worldPoint.y;
                        topPointOnSide = worldPoint;
                    }
                }
            }
        }

        // Если не нашли точек на нужной стороне, используем ближайшую точку
        if (float.IsNegativeInfinity(highestY))
        {
            return closestPoint;
        }

        return topPointOnSide;
    }

}

