using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class DecorData : MonoBehaviour
{
    
    [SerializeField] private DecorType _decorType;
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
    }
    private void UpdateSortingOrder()
    {
        // Получаем Y-смещение в зависимости от типа декора
        float yOffset = YOffset(transform.position.y);

        // Используем смещение при расчете позиции
        float adjustedY = transform.position.y + yOffset;
        float deltaY = _playerMovement.transform.position.y - adjustedY;
        int offset = Mathf.RoundToInt(deltaY);

        _spriteRenderer.sortingOrder = _playerSpriteRenderer.sortingOrder + offset;

        // Вычисляем разницу высоты (игрок выше объекта)
        float heightDifference = _playerMovement.transform.position.y - adjustedY;

        var color = _spriteRenderer.color;
        float finalAlpha = 1f;

        if (heightDifference > 0)
        {
            // --- Вертикальная часть (как было) ---
            float minHeight = transform.localScale.y;
            float midHeight = transform.localScale.y * 5f;
            float maxHeight = transform.localScale.y * 10f;
            float minAlpha = 0.3f;

            float verticalAlpha;
            if (heightDifference <= minHeight)
            {
                verticalAlpha = 1f;
            }
            else if (heightDifference <= midHeight)
            {
                float progress = (heightDifference - minHeight) / (midHeight - minHeight);
                verticalAlpha = Mathf.Lerp(1f, minAlpha, progress);
            }
            else if (heightDifference <= maxHeight)
            {
                float progress = (heightDifference - midHeight) / (maxHeight - midHeight);
                verticalAlpha = Mathf.Lerp(minAlpha, 1f, progress);
            }
            else
            {
                verticalAlpha = 1f;
            }

            // --- Горизонтальная часть (новое: плавное появление по бокам) ---
            // Используем реальные размеры спрайта в мировых координатах
            float halfWidth = _spriteRenderer.bounds.extents.x;

            // Зона полного эффекта — внутри спрайта, зона затухания — дополнительно снаружи
            float extraFadeWidth = halfWidth * 3f; // Настраиваемый множитель: чем больше — тем шире зона затухания по бокам
            float maxHorizontalDist = halfWidth + extraFadeWidth;

            float horizontalDist = Mathf.Abs(_playerMovement.transform.position.x - transform.position.x);
            float sideProgress = Mathf.Clamp01(horizontalDist / maxHorizontalDist);
            float sideFactor = 1f - sideProgress; // 1 — в центре (полный эффект), 0 — на краю зоны и дальше

            // Итоговая альфа: плавно смешиваем вертикальный эффект с полной непрозрачностью
            finalAlpha = Mathf.Lerp(1f, verticalAlpha, sideFactor);
        }
        else
        {
            finalAlpha = 1f;
        }

        color.a = Mathf.Clamp01(finalAlpha);
        _spriteRenderer.color = color;
    }

    private float YOffset(float posY)
    {
        switch (_decorType)
        {
            case DecorType.tree: return -transform.localScale.y*5f;
            case DecorType.house: return -transform.localScale.y * 1.3f;
            case DecorType.bigHouse: return -transform.localScale.y * 1.7f;
            case DecorType.police: return -transform.localScale.y * 1.7f;
            case DecorType.store: return -transform.localScale.y * 1.6f;
            case DecorType.bigStore: return -transform.localScale.y * 1.7f;
            case DecorType.hospital: return -transform.localScale.y * 1.7f;
            default: return 0;
        }
    }
}

public enum DecorType
{
    tree,
    house,
    bigHouse,
    store,
    bigStore,
    police,
    hospital
}
