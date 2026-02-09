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
        // ѕолучаем Y-смещение в зависимости от типа декора
        float yOffset = YOffset(transform.position.y);

        // »спользуем смещение при расчете позиции
        float adjustedY = transform.position.y + yOffset;
        float deltaY = _playerMovement.transform.position.y - adjustedY;
        int offset = Mathf.RoundToInt(deltaY);

        _spriteRenderer.sortingOrder = _playerSpriteRenderer.sortingOrder + offset;

        if (adjustedY < _playerMovement.transform.position.y)
        {
            var color = _spriteRenderer.color;
            color.a = 0.3f;
            _spriteRenderer.color = color;
        }
        else
        {
            var color = _spriteRenderer.color;
            color.a = 1f;
            _spriteRenderer.color = color;
        }
    }

    private float YOffset(float posY)
    {
        switch (_decorType)
        {
            case DecorType.tree: return -transform.localScale.y*5f;
            case DecorType.house: return -transform.localScale.y * 1f;
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
    police
}
