using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class BuildingBehaviour : MonoBehaviour
{
    
    public Vector2Int GridPosition = new Vector2Int();

    public Vector2Int Size = new Vector2Int();

    public BuildingType baseType;
    public BuildingLevelType levelType;

    public bool isAheadOfThePlayer = true;

    public float safeDistance = 1f;
    public float pushForce = 2f;
    public float teleportDuration = 0.5f;

    private Transform _target;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _playerSpriteRenderer;
    private UltraSensitiveDirectionController _playerController;

    private PlayerMovement _playerMovement;
    private DiContainer _diContainer;

    private bool _isPlayerInDangerZone;
    private bool _isPushing;
    private int _defaultSortingOrder;
    private Coroutine _enableControllerCoroutine;

    private bool _isStart;


    [Inject] private CollectorDataSO _collectorDataSO;


    public void Construct(PlayerMovement playerMovement)
    {
        _playerMovement = playerMovement;

        Initialize();
    }

    private void Initialize()
    {

        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isStart = true;
    }

    private void Update()
    {
        if (!_isStart) return;

        if (isAheadOfThePlayer)
            HandleSortingOrder();
    }

    private void HandleSortingOrder()
    {
        if (_spriteRenderer == null || _playerSpriteRenderer == null) return;

        if (_target.position.y > transform.position.y)
        {
            _spriteRenderer.sortingOrder = _playerSpriteRenderer.sortingOrder + 1;
        }
        else
        {
            _spriteRenderer.sortingOrder = _playerSpriteRenderer.sortingOrder - 1;
        }
    }

    private void FindPlayer()
    {
        _target = _playerMovement.transform;

        if (_target != null)
        {
            _playerSpriteRenderer = _target.GetComponent<SpriteRenderer>();
        }
    }

    private void OnDisable()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = _defaultSortingOrder;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, safeDistance);
    }

}