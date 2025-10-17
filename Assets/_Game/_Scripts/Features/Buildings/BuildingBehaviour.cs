using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;
using Zenject;

public class BuildingBehaviour : MonoBehaviour
{
    public BuildingType baseType;
    public bool isAheadOfThePlayer = true;

    public float safeDistance = 1f;
    public float pushForce = 2f;
    public float teleportDuration = 0.5f;

    private Transform _target;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _playerSpriteRenderer;
    private UltraSensitiveDirectionController _playerController;
    private PlayerMovement _playerMovement;

    private bool _isPlayerInDangerZone;
    private bool _isPushing;
    private int _defaultSortingOrder;
    private Coroutine _enableControllerCoroutine;

    private void Start()
    {

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _defaultSortingOrder = _spriteRenderer.sortingOrder;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    private void Update()
    {
        HandlePlayerDetection();

        if(isAheadOfThePlayer)
            HandleSortingOrder();
    }

    private void HandlePlayerDetection()
    {
        if (_target == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector2.Distance(transform.position, _target.position);

        if (distance < safeDistance && !_isPlayerInDangerZone && !_isPushing)
        {
            EnterDangerZone();
        }
        else if (distance >= safeDistance && _isPlayerInDangerZone && !_isPushing)
        {
            ExitDangerZone();
        }
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
        _target = PlayerInstaller.entityList
            .Select(x => x.transform)
            .FirstOrDefault(x => x.GetComponent<PlayerMovement>() != null);

        if (_target != null)
        {
            _playerController = _target.GetComponent<UltraSensitiveDirectionController>();
            _playerMovement = _target.GetComponent<PlayerMovement>();
            _playerSpriteRenderer = _target.GetComponent<SpriteRenderer>();
        }
    }

    private void EnterDangerZone()
    {
        _isPlayerInDangerZone = true;
        _isPushing = true;

        if (_enableControllerCoroutine != null)
        {
            StopCoroutine(_enableControllerCoroutine);
            _enableControllerCoroutine = null;
        }

        if (_playerController != null)
        {
            _playerController.enabled = false;
        }

        PushPlayerAway();
    }

    private void ExitDangerZone()
    {
        _isPlayerInDangerZone = false;

        if (_playerController != null && !_playerController.enabled)
        {
            _enableControllerCoroutine = StartCoroutine(EnableControllerAfterDelay(0.1f));
        }
    }

    private IEnumerator EnableControllerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_target != null && !_isPlayerInDangerZone && !_isPushing && _playerController != null)
        {
            _playerController.enabled = true;
        }

        _enableControllerCoroutine = null;
    }

    private void PushPlayerAway()
    {
        if (_target == null) return;

        Vector3 direction = (_target.position - transform.position).normalized;
        Vector3 targetPosition = transform.position + direction * (safeDistance + 0.2f);

        _target.DOMove(targetPosition, teleportDuration)
              .SetEase(Ease.OutCubic)
              .OnComplete(() => {
                  _isPushing = false;

                  float distanceAfterPush = Vector2.Distance(transform.position, _target.position);
                  if (distanceAfterPush < safeDistance)
                  {
                      PushPlayerAway();
                  }
                  else
                  {
                      _isPlayerInDangerZone = false;
                      ExitDangerZone();
                  }
              });
    }

    private void OnDisable()
    {
        if (_playerController != null)
        {
            _playerController.enabled = true;
        }

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = _defaultSortingOrder;
        }

        if (_enableControllerCoroutine != null)
        {
            StopCoroutine(_enableControllerCoroutine);
            _enableControllerCoroutine = null;
        }

        _isPushing = false;
        _isPlayerInDangerZone = false;
    }

    private void OnDestroy()
    {
        if (_playerController != null)
        {
            _playerController.enabled = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, safeDistance);
    }
}